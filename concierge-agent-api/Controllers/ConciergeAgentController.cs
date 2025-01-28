using Asp.Versioning;
using concierge_agent_api.Models;
using concierge_agent_api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel;
using System.Net.Mime;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;

namespace concierge_agent_api.Controllers;

[ApiVersion("1.0")]
[ApiController]
[Route("api/v{v:apiVersion}/[controller]")]
public class ConciergeAgentController : ControllerBase
{
    private readonly ILogger<ConciergeAgentController> _logger;
    private readonly IAzureDatabricksService _azureDatabricksService;
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chat;
    private readonly IChatHistoryManager _chatHistoryManager;
    private readonly IMemoryCache _memoryCache;

    public ConciergeAgentController(
        ILogger<ConciergeAgentController> logger,
        IAzureDatabricksService azureDatabricksService,
        Kernel kernel,
        IChatCompletionService chat,
        IChatHistoryManager chathistorymanager,
        IMemoryCache memoryCache)
    {
        _kernel = kernel;
        _chat = chat;
        _chatHistoryManager = chathistorymanager;
        _logger = logger;
        _azureDatabricksService = azureDatabricksService;
        _memoryCache = memoryCache;
    }

    [MapToApiVersion("1.0")]
    [HttpPost("ticketing")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> InitiateEventWorkflow([FromBody] EventWorkflowRequest request)
    {
        try
        {
            if (ModelState.IsValid == false)
            {
                return BadRequest(ModelState);
            }

            await CacheLotLocations(request.TMEventId);
            await CacheLotLookup();
            var jsonRequest = JsonSerializer.Serialize(request);
            _logger.LogInformation($"Initiating event workflow for request: {jsonRequest}");

            var customer = await _azureDatabricksService.GetCustomerByEmailAsync(request.TMEmail);
            var eventMaster = await _azureDatabricksService.GetEventMasterAsync(request.TMEventId);

            var chatHistory = _chatHistoryManager.GetOrCreateChatHistory(request.SmsNumber);

            // check the event type so we know if this is a game, event, or a concert
            string gameOrEventText = string.Empty;
            switch (eventMaster.EventType.ToLower())
            {
                case "mbs events":
                    gameOrEventText = "event";
                    break;
                case "mbs concerts":
                    gameOrEventText = "concert";
                    break;
                default:
                    gameOrEventText = "game";
                    break;
            }

            var initialMessage = $"Hello {customer.FirstName}, We're excited to see you at the {eventMaster.TMEventNameLong} {gameOrEventText} on {eventMaster.EventDate}. Are you planning to drive, use rideshare, or take public transit?";

            // TODO: send initial text message to the customer
            chatHistory.AddSystemMessage(initialMessage);
            chatHistory.AddSystemMessage($"TMEventId:{request.TMEventId}"); // needed to look up event
            _logger.LogInformation(initialMessage);

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, string.Empty);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    private async Task CacheLotLocations(string tmEventId)
    {
        if (!_memoryCache.TryGetValue("LotLocations", out List<LotLocation> lotLocations))
        {
            lotLocations = await _azureDatabricksService.GetLotLocationsAsync(true, tmEventId);
            _memoryCache.Set($"LotLocations-{tmEventId}", lotLocations, TimeSpan.FromMinutes(120));
        }
    }

    private async Task CacheLotLookup()
    {
        if (!_memoryCache.TryGetValue("LotLookup", out List<LotLookup> lotLookup))
        {
            lotLookup = await _azureDatabricksService.GetLotLookupAsync();
            _memoryCache.Set("LotLookup", lotLookup, TimeSpan.FromMinutes(120));
        }
    }
}