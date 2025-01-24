using concierge_agent_api.Models;

namespace concierge_agent_api.Services;

public interface ICosmosDbChatHistoryManager
{
    Session GetOrCreateChatHistoryAsync(string sessionId);
}

public class CosmosDbChatHistoryManager : ICosmosDbChatHistoryManager
{
    private readonly ICosmosDbService _cosmosDbService;

    public CosmosDbChatHistoryManager(ICosmosDbService cosmosDbService)
    {
        _cosmosDbService = cosmosDbService;
    }

    public Session GetOrCreateChatHistoryAsync(string sessionId)
    {
        throw new NotImplementedException();
    }
}