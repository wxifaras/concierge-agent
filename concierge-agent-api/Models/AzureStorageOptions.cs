namespace concierge_agent_api.Models;

public class AzureStorageOptions
{
    public const string AzureStorage = "AzureStorageOptions";

    public string QueueConnectionString { get; set; }
    public string QueueName { get; set; }
    public string StorageConnectionString { get; set; }
}
