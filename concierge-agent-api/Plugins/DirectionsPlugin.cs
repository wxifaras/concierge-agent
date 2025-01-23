using concierge_agent_api.Services;

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
}