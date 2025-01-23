namespace concierge_agent_api.Services;

using Azure.Core;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using concierge_agent_api.Models;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.AspNetCore.Mvc;
using System.Drawing.Text;

public class SmsQueueProcessor : BackgroundService
{
    private QueueClient _queueClient;
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chat;
    private readonly IChatHistoryManager _chatHistoryManager;
    private readonly ILogger<SmsQueueProcessor> _logger;

    public SmsQueueProcessor(
        IOptions<AzureStorageOptions> options,
        Kernel kernel,
        IChatCompletionService chat,
        IChatHistoryManager chathistorymanager,
        ILogger<SmsQueueProcessor> logger)
    {
        if (string.IsNullOrWhiteSpace(options.Value.ConnectionString))
            throw new ArgumentException("Azure Storage Connection String is not configured.", nameof(options));

        if (string.IsNullOrWhiteSpace(options.Value.QueueName))
            throw new ArgumentException("Azure Storage Queue Name is not configured.", nameof(options));

        _queueClient = new QueueClient(options.Value.ConnectionString, options.Value.QueueName);
        _chat = chat;
        _kernel = kernel;
        _chatHistoryManager = chathistorymanager;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var response = await _queueClient.ReceiveMessagesAsync(maxMessages: 10);

                foreach (QueueMessage queueMessage in response.Value)
                {
                    try
                    {
                        // "Process" the message
                        _logger.LogInformation($"Message: {queueMessage.MessageText}");

                        //byte[] data = Convert.FromBase64String(queueMessage.MessageText);
                        //string json = Encoding.UTF8.GetString(data);

                        // Parse the JSON
                        //JArray jsonArray = JArray.Parse(json);
                        JArray jsonArray = JArray.Parse(queueMessage.MessageText);
                        JObject jsonObject = jsonArray[0] as JObject;

                        // Dynamically access top-level properties
                        string id = (string)jsonObject["id"];
                        string eventType = (string)jsonObject["eventType"];

                        // Dynamically access nested data properties
                        JObject dataObject = (JObject)jsonObject["data"];
                        string messageId = (string)dataObject["MessageId"];
                        string fromSmsNumber = (string)dataObject["From"];
                        string message = (string)dataObject["Message"];

                        // Delete the message after successful processing
                        await _queueClient.DeleteMessageAsync(queueMessage.MessageId, queueMessage.PopReceipt);

                        await ProcessMessageAsync(fromSmsNumber, message);

                    }
                    catch (Exception messageProcessingException)
                    {
                        _logger.LogError($"Error processing message: {messageProcessingException.Message}");
                    }
                }

                // Add a small delay to prevent tight looping
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error receiving messages: {ex.Message}");

                // Prevent tight error loop
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }
    }
    
    private async Task ProcessMessageAsync(string smsNumber, string message)
    {
        // get the chat history for this user based on their phone number and initiate AI process
        var chatHistory = _chatHistoryManager.GetOrCreateChatHistory(smsNumber);
        chatHistory.AddUserMessage(message);

        // Process the message

        ChatMessageContent? result = await _chat.GetChatMessageContentAsync(
             chatHistory,
             executionSettings: new OpenAIPromptExecutionSettings { Temperature = 0.0, TopP = 0.0, ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions },
             kernel: _kernel);

        // TODO: send message back to user via SMS

        _logger.LogInformation(result.Content);
    }
}