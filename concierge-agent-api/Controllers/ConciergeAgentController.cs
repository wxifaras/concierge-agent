using Asp.Versioning;
using concierge_agent_api.Models;
using concierge_agent_api.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

namespace concierge_agent_api.Controllers
{
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/v{v:apiVersion}/[controller]")]
    public class ConciergeAgentController : ControllerBase
    {
        private readonly ILogger<ConciergeAgentController> _logger;
        private readonly IAzureDatabricksService _azureDatabricksService;

        public ConciergeAgentController(
            ILogger<ConciergeAgentController> logger,
            IAzureDatabricksService azureDatabricksService)
        {
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

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, string.Empty);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}