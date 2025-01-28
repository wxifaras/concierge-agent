using Azure.Storage.Blobs;
using concierge_agent_api.Models;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace concierge_agent_api.Plugins;

public class EventsPlugin
{
    private readonly ILogger<EventsPlugin> _logger;
    private readonly string _connectionString;

    public EventsPlugin(
        ILogger<EventsPlugin> logger, 
        IOptions<AzureStorageOptions> options)
    {
        _logger = logger;
        _connectionString = options.Value.StorageConnectionString;
    }

    [KernelFunction("get_event_information")]
    [Description("Gets event related information.")]
    public async Task<string> GetEventInfo(
    [Description("The TMEventId of the event.")] string tmEventId)
    {
        _logger.LogInformation($"get_event_information");

        var blobServiceClient = new BlobServiceClient(_connectionString);
        var containerClient = blobServiceClient.GetBlobContainerClient($"events/{tmEventId}");
        var blobClient = containerClient.GetBlobClient($"{tmEventId}.txt");

        var response = await blobClient.DownloadContentAsync();
        var content = response.Value.Content.ToString();

        return content;
    }
}
