using concierge_agent_api.Models;
using concierge_agent_api.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.SemanticKernel;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Drawing.Text;
using System.Text.RegularExpressions;

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

    [KernelFunction("get_latitude_and_longitude_and_address")]
    [Description("Gets the latitude, longitude, and new address from the specified origin")]
    public async Task<string> GetLatitudeAndLongitude(
         [Description("The origin of where the customer will be coming from")] string origin,
         [Description("The current address of the origin")] string originAddress,
         [Description("A flag to determine if the origin a business or not")] bool originFlag)
    {
        _logger.LogInformation($"getlatitudeandlongitude");

        string latitude= String.Empty;
        string longitude= String.Empty;
        string originMapsAddress= String.Empty; 
        //right now we are just returning the 1st returned result from Azure Maps. Would want to check that the correct business is found.
        //Would want to do the same for address. 
        //radius is set to 40miles from stadium
        if (originFlag)
        {
            PointOfInterest pointOfInterest = await _azureMapsService.GetPointOfInterestAsync(origin, 33.75528, -84.40083, 64374);
            latitude = pointOfInterest.Results[0].Position.Lat.ToString();
            longitude = pointOfInterest.Results[0].Position.Lon.ToString();
            originMapsAddress = pointOfInterest.Results[0].Address.FreeformAddress.ToString();  
        }
        else
        {
            LatLongLookUp latLongLookUp = await _azureMapsService.GetLatLongAsync(origin);
            latitude = latLongLookUp.Results[0].Position.Lat.ToString();
            longitude = latLongLookUp.Results[0].Position.Lon.ToString();
            originMapsAddress = latLongLookUp.Results[0].Address.FreeformAddress.ToString();
        }

        var mapsaddressLatLong = $"{originMapsAddress} {latitude} {longitude}";

        return mapsaddressLatLong;
    }

    [KernelFunction("get_distance_to_destination")]
    [Description("Calculates the distance from the customer's origin to the specified destination")]
    public async Task<double> GetDistanceToDestination(
        [Description("The origin of where the customer will be driving from to the destination")] string origin,
        [Description("The address of the origin")] string originAddress,
        [Description("The latitude of the origin")] string originLatitude,
        [Description("The longitude of the origin")] string originLongitude,
        [Description("The address of the destination")] string destinationAddress,
        [Description("The latitude of the destination")] string destinationLatitude,
        [Description("The longitude of the destination")] string destinationLongitude,
        [Description("The travel mode to the stadium")] TravelMode travelMode)
    {
        _logger.LogInformation($"get_distance_to_destination");

        double distance = await _azureMapsService.GetDistanceInMilesAsync(Double.Parse(originLatitude), Double.Parse(originLongitude), Double.Parse(destinationLatitude), Double.Parse(destinationLongitude), travelMode);

        return distance;
    }

    [KernelFunction("get_directions_to_destination")]
    [Description("Returns a link for directions from the customer's origin to the specified destination")]
    public async Task<string> GetDirectionsToDestination(
        [Description("The origin of where the customer will be driving from to the destination")] string origin,
        [Description("The address of the origin")] string originAddress,
        [Description("The latitude of the origin")] string originLatitude,
        [Description("The longitude of the origin")] string originLongitude,
        [Description("The address of the destination")] string destinationAddress,
        [Description("The latitude of the destination")] string destinationLatitude,
        [Description("The longitude of destination")] string destinationLongitude)
    {
        _logger.LogInformation($"get_directions_to_destination");

        RouteSummary routeSummary = await _azureMapsService.GetDirectionsAsync(Double.Parse(originLatitude), Double.Parse(originLongitude), Double.Parse(destinationLatitude), Double.Parse(destinationLongitude), TravelMode.car);

        return routeSummary.mapLink;
    }

    [KernelFunction("get_parking_options")]
    [Description("Returns the parking options to include the distance of each option to the Mercedes-Benz Stadium.")]
    public async Task<string> GetParkingOptions(
        [Description("The address of Mercedes-Benz Stadium")] string destinationAddress,
        [Description("The latitude of Mercedes-Benz Stadium")] string destinationLatitude,
        [Description("The longitude of Mercedes-Benz Stadium")] string destinationLongitude,
        [Description("The TMEventId of the event")] string tmEventId/*,
        [Description("Whether the customer is open to a short walk or not")] bool isOpenToShortWalk*/
        )
    {
        _logger.LogInformation($"get_parking_options");
        List<LotLocation> lotLocations = _memoryCache.Get<List<LotLocation>>($"LotLocations-{tmEventId}");
        var enrichedJson = string.Empty;

        if (lotLocations != null)
        {
            if (!_memoryCache.TryGetValue("EnrichedLotLocations", out List<EnrichedLotLocation> enrichedLotLocations))
            {
                enrichedLotLocations = new List<EnrichedLotLocation>();

                // based on whether the customer is open to a short walk or not, which parking recommendations to provide
                foreach (LotLocation lotLocation in lotLocations)
                {
                    var distanceToStadium = lotLocation.dist;

                    // if the distance to the stadium for this lot is not in the database, get it from the map service. Note that the map service may
                    // give a longer distance because it may take roads to get to the stadium even if it's located directly beside the stadium
                    if (distanceToStadium == null)
                    {
                        double distance = await _azureMapsService.GetDistanceInMilesAsync(lotLocation.lat, lotLocation.longitude, double.Parse(destinationLatitude), double.Parse(destinationLongitude), TravelMode.pedestrian);
                        distanceToStadium = distance.ToString();
                    }

                    var enrichedLotLocation = new EnrichedLotLocation
                    {
                        lot_lat = lotLocation.lat.ToString(),
                        lot_long = lotLocation.longitude.ToString(),
                        actual_lot = lotLocation.actual_lot,
                        location_type = lotLocation.locationType,
                        distance_to_stadium = distanceToStadium.ToString(),
                        amenities = lotLocation.amenities,
                        description = lotLocation.desc,
                        lot_price = lotLocation.lot_price
                    };

                    enrichedLotLocations.Add(enrichedLotLocation);
                }

                _memoryCache.Set("EnrichedLotLocations", enrichedLotLocations, TimeSpan.FromMinutes(120));
            }

            var sortedEnrichedLotLocations = enrichedLotLocations
                .OrderBy(lot => ParseDistance(lot.distance_to_stadium))
                .ToList();

            enrichedJson = JsonConvert.SerializeObject(sortedEnrichedLotLocations, Formatting.Indented);
        }

        return enrichedJson;
    }

    /// <summary>
    /// Parses the distance from a string, which wil be either a numerical value or a descriptive string.
    /// </summary>
    /// <param name="distance"></param>
    /// <returns></returns>
    private double ParseDistance(string distance)
    {
        // Check if the distance includes a description like "miles from stadium". All of the dist fields have this description,
        // so this is dependent on the data
        var match = Regex.Match(distance, @"([0-9.]+)\s*miles? from stadium", RegexOptions.IgnoreCase);
        if (match.Success)
        {
            return double.Parse(match.Groups[1].Value); // Extract and return the numeric part
        }

        // If it's just a number, assume it's already in miles
        return double.Parse(distance);
    }

    [KernelFunction("get_closest_marta_station")]
    [Description("Returns the closest MARTA station to the customers origin location")]
    public async Task<string> GetClosestMartaStation(
        [Description("The origin of where the customer will be driving from to the stadium")] string origin,
        [Description("The address of the origin")] string originAddress,
        [Description("The latitude of the origin")] string originLatitude,
        [Description("The longitude of the origin")] string originLongitude,
        [Description("JSON list of MARTA stations closest to the customer")] string jsonMartaStations)
    {
        _logger.LogInformation($"get_closest_marta_station");

        var stationList = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(jsonMartaStations);

        // add the distance to each MARTA station from the customer's origin
        foreach (var station in stationList)
        {
            var stationLat = station["station_lat"].ToString();
            var stationLong = station["station_long"].ToString();
            
            double distanceToStation = await _azureMapsService.GetDistanceInMilesAsync(double.Parse(originLatitude), double.Parse(originLongitude), double.Parse(stationLat), double.Parse(stationLong), TravelMode.car);
            station["distanceToStation"] = distanceToStation;
        }

        var strStationList = JsonConvert.SerializeObject(stationList, Formatting.Indented);

        return strStationList;
    }

    [KernelFunction("get_weather")]
    [Description("Gets the current weather at a specified location")]
    public async Task<string> GetClosestMartaStation(
        [Description("The latitude of the specified location")] string latitude,
        [Description("The longitude of the specified location")] string longitude)
    {
        _logger.LogInformation($"get_weather");

        var weather = await _azureMapsService.GetWeatherAsync(double.Parse(latitude), double.Parse(longitude));

        return weather;
    }
}