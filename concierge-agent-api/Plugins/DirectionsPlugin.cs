using concierge_agent_api.Models;
using concierge_agent_api.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.SemanticKernel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.ComponentModel;

namespace concierge_agent_api.Plugins;

public class DirectionsPlugin
{
    private readonly ILogger<DirectionsPlugin> _logger;
    private readonly IAzureDatabricksService _azureDatabricksService;
    private readonly IAzureMapsService _azureMapsService;
    private readonly IMemoryCache _memoryCache;

    public DirectionsPlugin(
        ILogger<DirectionsPlugin> logger,
        IAzureDatabricksService azureDatabricksService,
        IAzureMapsService azureMapsService,
        IMemoryCache memoryCache)
    {
        _logger = logger;
        _azureDatabricksService = azureDatabricksService;
        _azureMapsService = azureMapsService;
        _memoryCache = memoryCache;
    }

    [KernelFunction("get_distance_to_stadium")]
    [Description("Calculates the distance from the customer's origin to Mercedes-Benz Stadium")]
    public async Task<string> GetDistanceToStadium(
        [Description("The origin of where the customer will be driving from to the stadium")] string origin,
        [Description("The address of the origin")] string originAddress,
        [Description("The latitude of the origin")] string originLatitude,
        [Description("The longitude of the origin")] string originLongitude,
        [Description("The address of Mercedes-Benz Stadium")] string destinationAddress,
        [Description("The latitude of Mercedes-Benz Stadium")] string destinationLatitude,
        [Description("The longitude of Mercedes-Benz Stadium")] string destinationLongitude
        )
    {
        _logger.LogInformation($"get_distance_to_stadium");
        //return $"Directions to stadium from {origin} ({originAddress}) are as follows...";
        string directions = await _azureMapsService.GetDirectionsAsync(Double.Parse(originLatitude), Double.Parse(originLongitude), Double.Parse(destinationLatitude), Double.Parse(destinationLongitude));

        return directions;
    }

    [KernelFunction("get_parking_options")]
    [Description("Returns the parking options to include the distance of each option to the Mercedes-Benz Stadium.")]
    public async Task<string> GetParkingOptions(
        [Description("The address of Mercedes-Benz Stadium")] string destinationAddress,
        [Description("The latitude of Mercedes-Benz Stadium")] string destinationLatitude,
        [Description("The longitude of Mercedes-Benz Stadium")] string destinationLongitude/*,
        [Description("Whether the customer is open to a short walk or not")] bool isOpenToShortWalk*/
        )
    {
        _logger.LogInformation($"get_parking_options");
        List<LotLocation> lotLocations = _memoryCache.Get<List<LotLocation>>("lotLocations");

        // TODO IF NEEDED: pull from the cache each lot location with the corresponding calculated the distance to the stadium and add to a json structure we can return so the LLM can decide,
        List<JObject> jsonObjectsList = new List<JObject>();

        // based on whether the customer is open to a short walk or not, which parking recommendations to provide
        foreach (LotLocation lotLocation in lotLocations)
        {
            string directionSummary = await _azureMapsService.GetDirectionsAsync(lotLocation.lat, lotLocation.longitude, Double.Parse(destinationLatitude), Double.Parse(destinationLongitude));

            string distanceToStadium = JObject.Parse(directionSummary)["lengthInMeters"].ToString();

            var jsonObject = new JObject
            {
                { "lot_lat", lotLocation.lat },
                { "lot_long", lotLocation.longitude },
                { "actual_lot", lotLocation.actual_lot },
                { "location_type", lotLocation.locationType },
                { "distance_to_stadium_in_meters",  distanceToStadium }
            };

            jsonObjectsList.Add(jsonObject);
        }

        string jsonString = JsonConvert.SerializeObject(jsonObjectsList);

        return jsonString;
    }
}