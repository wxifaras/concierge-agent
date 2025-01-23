using Asp.Versioning;
using concierge_agent_api.Models;
using concierge_agent_api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel;
using System.Net.Mime;

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

    public ConciergeAgentController(
        ILogger<ConciergeAgentController> logger,
        IAzureDatabricksService azureDatabricksService,
        Kernel kernel,
        IChatCompletionService chat,
        IChatHistoryManager chathistorymanager)
    {
        _kernel = kernel;
        _chat = chat;
        _chatHistoryManager = chathistorymanager;
        _logger = logger;
        _azureDatabricksService = azureDatabricksService;
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

            var customer = await _azureDatabricksService.GetCustomerByEmailAsync(request.TMEmail);
            var eventMaster = await _azureDatabricksService.GetEventMasterAsync(request.TMEventId);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, string.Empty);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}