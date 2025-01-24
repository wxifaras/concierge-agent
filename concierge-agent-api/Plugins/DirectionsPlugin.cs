using concierge_agent_api.Models;
using concierge_agent_api.Services;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace concierge_agent_api.Plugins;

public class DirectionsPlugin
{
    private readonly ILogger<DirectionsPlugin> _logger;
    private readonly IAzureDatabricksService _azureDatabricksService;

    public DirectionsPlugin(
        ILogger<DirectionsPlugin> logger,
        IAzureDatabricksService azureDatabricksService)
    {
        _logger = logger;
        _azureDatabricksService = azureDatabricksService;
    }

    [KernelFunction("get_distance_to_stadium")]
    [Description("Calculates the distance from the customer's origin to Mercedes-Benz Stadium")]
    public string GetDistanceToStadium(
        [Description("The origin of where the customer will be driving from to the stadium")] string origin,
        [Description("The address of the origin")] string address
        )
    {
        // TODO: use mapping service to calculate distance and return results
        return $"Directions to stadium from {origin} ({address}) are as follows...";
    }

    [KernelFunction("get_closest_parking_recommendations")]
    [Description("Returns the top three closest parking recommendations")]
    public async Task<List<LotLocation>> GetParkingRecommendations(
        [Description("The origin of where the customer will be driving from to the stadium")] string origin,
        [Description("The address of the origin")] string address
        )
    {
        List<LotLocation> lotLocations = await _azureDatabricksService.GetLotLocationsAsync(true);
        return lotLocations;
        //return $"Getting parking recommendations";
    }
}