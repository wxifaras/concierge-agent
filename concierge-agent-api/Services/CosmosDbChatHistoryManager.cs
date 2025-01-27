using concierge_agent_api.Models;

namespace concierge_agent_api.Services;

public interface ICosmosDbChatHistoryManager
{
    Task<Session> GetOrCreateChatHistoryAsync(Session session);
}

public class CosmosDbChatHistoryManager : ICosmosDbChatHistoryManager
{
    private readonly ICosmosDbService _cosmosDbService;

    public CosmosDbChatHistoryManager(ICosmosDbService cosmosDbService)
    {
        _cosmosDbService = cosmosDbService;
    }

    public async Task<Session> GetOrCreateChatHistoryAsync(Session session)
    {
        return await _cosmosDbService.InsertSessionAsync(session);
    }
}