using Azure.Identity;
using concierge_agent_api.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel.ChatCompletion;
using Newtonsoft.Json;

namespace concierge_agent_api.Services;

public class ChatHistoryItem
{
    [JsonProperty(PropertyName = "id")]
    public string Id { get; set; }
    public string SmsNumber { get; set; } // partition key
    public string TmEventId { get; set; } // partition key
    public string? UserId { get; set; }
    public List<ChatMessageDto> Messages { get; set; }
    public DateTime LastAccessed { get; set; }
}

public class ChatMessageDto
{
    public string Role { get; set; }
    public string Content { get; set; }
}

public interface ICosmosDbChatHistoryManager
{
    Task<ChatHistory> GetOrCreateChatHistoryAsync(string smsNumber, string tmEventId);
    Task SaveChatHistoryAsync(string smsNumber, string tmEventId, ChatHistory chatHistory);
    Task<ChatHistory> RetrieveChatHistoryAsync(string smsNumber, string tmEventId);
}

/// <summary>
/// The CosmosDbChatHistoryManager class is responsible for managing chat history in a Cosmos DB container.
/// Assumes a Hierarchical Partitioning scheme with the following partition key: /SmsNumber, /TmEventId
/// </summary>
public class CosmosDbChatHistoryManager : ICosmosDbChatHistoryManager
{
    private readonly Container _chatContainer;
    private readonly string _systemMessage;

    public CosmosDbChatHistoryManager(IOptions<CosmosDbOptions> options, string systemMessage)
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
        _systemMessage = systemMessage;
    }

    /// <summary>
    /// Retrieves an existing chat history or creates a new one if it doesn't exist.
    /// </summary>
    /// <param name="smsNumber">The SMS number associated with the chat history.</param>
    /// <param name="tmEventId">The event ID associated with the chat history.</param>
    /// <returns>A ChatHistory object.</returns>
    /// <example>
    /// Sample usage:
    /// <code>
    /// // Retrieve or create chat history
    /// var chatHistory = await cosmosDbChatHistoryManager.GetOrCreateChatHistoryAsync(request.SmsNumber, request.TMEventId);
    /// 
    /// // Add a system message (if needed)
    /// chatHistory.AddSystemMessage($"TMEventId:{request.TMEventId}");
    /// 
    /// // Save the updated chat history
    /// await cosmosDbChatHistoryManager.SaveChatHistoryAsync(request.SmsNumber, request.TMEventId, chatHistory);
    /// 
    /// // Add user and assistant messages
    /// chatHistory.AddUserMessage("User's new message");
    /// chatHistory.AddAssistantMessage("Assistant's response");
    /// 
    /// // Save the chat history again after adding new messages
    /// await cosmosDbChatHistoryManager.SaveChatHistoryAsync(request.SmsNumber, request.TMEventId, chatHistory);
    /// 
    /// // Retrieve the chat history (if needed later)
    /// chatHistory = await cosmosDbChatHistoryManager.RetrieveChatHistoryAsync(request.SmsNumber, request.TMEventId);
    /// </code>
    /// </example>
    public async Task<ChatHistory> GetOrCreateChatHistoryAsync(string smsNumber, string tmEventId)
    {
        var partitionKey = GetPK(smsNumber, tmEventId);

        try
        {
            var query = new QueryDefinition(
                "SELECT * FROM c WHERE c.SmsNumber = @smsNumber AND c.TmEventId = @tmEventId")
                .WithParameter("@smsNumber", smsNumber)
                .WithParameter("@tmEventId", tmEventId);

            using FeedIterator<ChatHistoryItem> feedIterator = _chatContainer.GetItemQueryIterator<ChatHistoryItem>(
                query,
                requestOptions: new QueryRequestOptions { PartitionKey = partitionKey }
            );

            if (feedIterator.HasMoreResults)
            {
                FeedResponse<ChatHistoryItem> response = await feedIterator.ReadNextAsync();
                if (response.Any())
                {
                    var item = response.First();
                    item.LastAccessed = DateTime.UtcNow;
                    await _chatContainer.ReplaceItemAsync(item, item.Id, partitionKey);

                    var chatHistory = new ChatHistory();
                    foreach (var message in item.Messages)
                    {
                        var role = message.Role.ToLowerInvariant() switch
                        {
                            "system" => AuthorRole.System,
                            "assistant" => AuthorRole.Assistant,
                            "user" => AuthorRole.User,
                            "tool" => AuthorRole.Tool,
                            _ => new AuthorRole(message.Role),
                        };

                        chatHistory.AddMessage(role, message.Content);
                    }

                    return chatHistory;
                }
            }

            // If no item found, create a new one
            var newChatHistory = CreateNewChatHistory();
            var newItem = new ChatHistoryItem
            {
                Id = Guid.NewGuid().ToString(),
                SmsNumber = smsNumber,
                TmEventId = tmEventId,
                Messages = new List<ChatMessageDto>
            {
                new ChatMessageDto
                {
                    Role = AuthorRole.System.ToString(),
                    Content = _systemMessage
                }
            },
                LastAccessed = DateTime.UtcNow
            };

            await _chatContainer.CreateItemAsync(newItem, partitionKey);
            return newChatHistory;
        }
        catch (CosmosException ex)
        {
            // Handle any other Cosmos DB exceptions here
            throw;
        }
    }

    /// <summary>
    /// Saves an updated chat history
    /// </summary>
    /// <param name="smsNumber"></param>
    /// <param name="tmEventId"></param>
    /// <param name="chatHistory"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task SaveChatHistoryAsync(string smsNumber, string tmEventId, ChatHistory chatHistory)
    {
        var partitionKey = GetPK(smsNumber, tmEventId);

        try
        {
            var query = new QueryDefinition(
                "SELECT * FROM c WHERE c.SmsNumber = @smsNumber AND c.TmEventId = @tmEventId")
                .WithParameter("@smsNumber", smsNumber)
                .WithParameter("@tmEventId", tmEventId);

            using FeedIterator<ChatHistoryItem> feedIterator = _chatContainer.GetItemQueryIterator<ChatHistoryItem>(
                query,
                requestOptions: new QueryRequestOptions { PartitionKey = partitionKey }
            );

            ChatHistoryItem? existingItem = null;
            if (feedIterator.HasMoreResults)
            {
                FeedResponse<ChatHistoryItem> response = await feedIterator.ReadNextAsync();
                existingItem = response.FirstOrDefault();
            }

            if (existingItem == null)
            {
                throw new InvalidOperationException("No existing chat history found for the given smsNumber and tmEventId.");
            }

            existingItem.Messages = chatHistory.Select(m => new ChatMessageDto
            {
                Role = m.Role.ToString(),
                Content = m.Content
            }).ToList();

            existingItem.LastAccessed = DateTime.UtcNow;

            await _chatContainer.UpsertItemAsync(existingItem, partitionKey);
        }
        catch (CosmosException ex)
        {
            // Handle any Cosmos DB exceptions here
            throw;
        }
    }

    /// <summary>
    /// Retrieves an existing chat history
    /// </summary>
    /// <param name="smsNumber"></param>
    /// <param name="tmEventId"></param>
    /// <returns></returns>
    public async Task<ChatHistory> RetrieveChatHistoryAsync(string smsNumber, string tmEventId)
    {
        var partitionKey = GetPK(smsNumber, tmEventId);

        try
        {
            var query = new QueryDefinition(
                "SELECT * FROM c WHERE c.SmsNumber = @smsNumber AND c.TmEventId = @tmEventId")
                .WithParameter("@smsNumber", smsNumber)
                .WithParameter("@tmEventId", tmEventId);

            using FeedIterator<ChatHistoryItem> feedIterator = _chatContainer.GetItemQueryIterator<ChatHistoryItem>(
                query,
                requestOptions: new QueryRequestOptions { PartitionKey = partitionKey }
            );

            if (feedIterator.HasMoreResults)
            {
                FeedResponse<ChatHistoryItem> response = await feedIterator.ReadNextAsync();
                if (response.Any())
                {
                    var item = response.First();
                    var chatHistory = new ChatHistory();
                    foreach (var message in item.Messages)
                    {
                        var role = message.Role.ToLowerInvariant() switch
                        {
                            "system" => AuthorRole.System,
                            "assistant" => AuthorRole.Assistant,
                            "user" => AuthorRole.User,
                            "tool" => AuthorRole.Tool,
                            _ => new AuthorRole(message.Role),
                        };

                        chatHistory.AddMessage(role, message.Content);
                    }

                    return chatHistory;
                }
            }

            return new ChatHistory();
        }
        catch (CosmosException ex)
        {
            // Handle any Cosmos DB exceptions here
            throw;
        }
    }

    private static PartitionKey GetPK(string smsNumber, string tmEventId)
    {
        if (string.IsNullOrEmpty(smsNumber) || string.IsNullOrEmpty(tmEventId))
        {
            throw new ArgumentException("Both smsNumber and tmEventId must be non-empty.");
        }

        return new PartitionKeyBuilder()
            .Add(smsNumber)
            .Add(tmEventId)
            .Build();
    }

    private ChatHistory CreateNewChatHistory()
    {
        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage(_systemMessage);
        return chatHistory;
    }
}