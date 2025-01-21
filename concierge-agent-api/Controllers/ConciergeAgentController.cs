using Microsoft.AspNetCore.Mvc;

namespace concierge_agent_api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ConciergeAgentController : ControllerBase
    {
        private readonly ILogger<ConciergeAgentController> _logger;

        public ConciergeAgentController(ILogger<ConciergeAgentController> logger)
        {
            _logger = logger;
        }
    }
}
