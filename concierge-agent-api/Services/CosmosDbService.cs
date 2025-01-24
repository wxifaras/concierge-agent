using Azure.Identity;
using concierge_agent_api.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;

namespace concierge_agent_api.Services;

public interface ICosmosDbService
{
    Task<Session> InsertSessionAsync(Session session);
    Task<List<Session>> GetSessionsAsync();
    Task<Session> UpdateSessionAsync(Session session);
    Task<Session> GetSessionAsync(string sessionId, string userId, string smsNumber);
}

public class CosmosDbService : ICosmosDbService
{
    private readonly Container _chatContainer;

    public CosmosDbService(IOptions<CosmosDbOptions> options)
    {
        CosmosClient cosmosClient = new(
           accountEndpoint: options.Value.AccountUri,
           tokenCredential: new DefaultAzureCredential(
               new DefaultAzureCredentialOptions
               {
                   TenantId = options.Value.TenantId,
                   ExcludeEnvironmentCredential = true
               })
       );

       _chatContainer = cosmosClient.GetContainer(options.Value.DatabaseName, options.Value.ContainerName);
    }

    /// <summary>
    /// Creates a new chat session.
    /// </summary>
    /// <param name="session">Chat session item to create.</param>
    /// <returns>Newly created chat session item.</returns>
    public async Task<Session> InsertSessionAsync(Session session)
    {
        PartitionKey partitionKey = GetPK(session.SessionId, session.UserId, session.SmsNumber);
        return await _chatContainer.CreateItemAsync<Session>(
            item: session,
            partitionKey: partitionKey
        );
    }

    /// <summary>
    /// Gets a list of all current chat sessions.
    /// </summary>
    /// <returns>List of distinct chat session items.</returns>
    public async Task<List<Session>> GetSessionsAsync()
    {
        QueryDefinition query = new QueryDefinition("SELECT DISTINCT * FROM c WHERE c.type = @type")
            .WithParameter("@type", nameof(Session));

        FeedIterator<Session> response = _chatContainer.GetItemQueryIterator<Session>(query);

        List<Session> output = new();
        while (response.HasMoreResults)
        {
            FeedResponse<Session> results = await response.ReadNextAsync();
            output.AddRange(results);
        }
        return output;
    }

    /// <summary>
    /// Updates an existing chat session.
    /// </summary>
    /// <param name="session">Chat session item to update.</param>
    /// <returns>Revised created chat session item.</returns>
    public async Task<Session> UpdateSessionAsync(Session session)
    {
        PartitionKey partitionKey = GetPK(session.SessionId, session.UserId, session.SmsNumber);
        return await _chatContainer.ReplaceItemAsync(
            item: session,
            id: session.Id,
            partitionKey: partitionKey
        );
    }

    /// <summary>
    /// Returns an existing chat session.
    /// </summary>
    /// <param name="sessionId">Chat session id for the session to return.</param>
    /// <returns>Chat session item.</returns>
    public async Task<Session> GetSessionAsync(string sessionId, string userId, string smsNumber)
    {
        PartitionKey partitionKey = GetPK(sessionId, userId, smsNumber);
        return await _chatContainer.ReadItemAsync<Session>(
            partitionKey: partitionKey,
            id: sessionId
            );
    }

    /// <summary>
    /// Helper function to generate a full or partial hierarchical partition key based on parameters.
    /// </summary>
    /// <param name="sessionId">Session Id of Chat/Session</param>
    /// <param name="userId">Id of User.</param>
    /// <param name="smsNumber">Sms Number of User</param>
    /// <returns>Newly created chat session item.</returns>
    private static PartitionKey GetPK(string sessionId, string userId, string smsNumber)
    {
        if (
            !string.IsNullOrEmpty(smsNumber)
            && !string.IsNullOrEmpty(userId)
            && !string.IsNullOrEmpty(sessionId)
        )
        {
            PartitionKey partitionKey = new PartitionKeyBuilder()
                .Add(smsNumber)
                .Add(userId)
                .Add(sessionId)
                .Build();

            return partitionKey;
        }
        else if (!string.IsNullOrEmpty(smsNumber) && !string.IsNullOrEmpty(userId))
        {
            PartitionKey partitionKey = new PartitionKeyBuilder().Add(smsNumber).Add(userId).Build();

            return partitionKey;
        }
        else
        {
            PartitionKey partitionKey = new PartitionKeyBuilder().Add(smsNumber).Build();

            return partitionKey;
        }
    }
}