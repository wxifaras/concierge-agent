namespace concierge_agent_api.Services;

using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using concierge_agent_api.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

public class SmsQueueProcessor : BackgroundService
{
    private QueueClient _queueClient;
    private readonly ILogger<SmsQueueProcessor> _logger;

    public SmsQueueProcessor(IOptions<AzureStorageOptions> options, ILogger<SmsQueueProcessor> logger)
    {
        if (string.IsNullOrWhiteSpace(options.Value.ConnectionString))
            throw new ArgumentException("Azure Storage Connection String is not configured.", nameof(options));

        if (string.IsNullOrWhiteSpace(options.Value.QueueName))
            throw new ArgumentException("Azure Storage Queue Name is not configured.", nameof(options));

        _queueClient = new QueueClient(options.Value.ConnectionString, options.Value.QueueName);
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

                        byte[] data = Convert.FromBase64String(queueMessage.MessageText);
                        string json = Encoding.UTF8.GetString(data);

                        // Parse the JSON
                        JArray jsonArray = JArray.Parse(json);
                        JObject jsonObject = jsonArray[0] as JObject;

                        // Dynamically access top-level properties
                        string id = (string)jsonObject["id"];
                        string eventType = (string)jsonObject["eventType"];

                        // Dynamically access nested data properties
                        JObject dataObject = (JObject)jsonObject["data"];
                        string messageId = (string)dataObject["MessageId"];
                        string from = (string)dataObject["From"];
                        string message = (string)dataObject["Message"];

                        // Delete the message after successful processing
                        await _queueClient.DeleteMessageAsync(queueMessage.MessageId, queueMessage.PopReceipt);
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

}