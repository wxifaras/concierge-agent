using concierge_agent_api.Services;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace concierge_agent_api.Plugins;

public class ParkingPlugin
{
    private readonly ILogger<DirectionsPlugin> _logger;
    private readonly IAzureDatabricksService _azureDatabricksService;

    public ParkingPlugin(
        ILogger<DirectionsPlugin> logger,
        IAzureDatabricksService azureDatabricksService)
    {
        _logger = logger;
        _azureDatabricksService = azureDatabricksService;
    }

    [KernelFunction("get_parking_recommendations")]
    [Description("Returns the top three parking recommendations")]
    public string GetParkingRecommendations()
    {
        return $"Getting parking recommendations";
    }
}