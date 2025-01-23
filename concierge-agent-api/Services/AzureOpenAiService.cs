using Azure;
using Azure.AI.OpenAI;
using concierge_agent_api.Models;
using Microsoft.Extensions.Options;
using OpenAI.Chat;

namespace concierge_agent_api.Services
{
    public class AzureOpenAiService
    {
        private ChatClient _chatClient;
        private ILogger<AzureOpenAiService> _logger;

        public AzureOpenAiService(IOptions<AzureOpenAiOptions> options, ILogger<AzureOpenAiService> logger)
        {
            AzureOpenAIClient azureClient = new(
                new Uri(options.Value.EndPoint), new AzureKeyCredential(options.Value.ApiKey));

            _chatClient = azureClient.GetChatClient(options.Value.DeploymentName);
            _logger = logger;
        }
    }
}
