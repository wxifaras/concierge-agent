using Newtonsoft.Json;

namespace concierge_agent_api.Models;

public record Session
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    [JsonProperty(PropertyName = "id")]
    public string Id { get; set; }

    public string SessionId { get; set; }

    public string UserId { get; set; }

    public string SmsNumber { get; set; }

    public string? Name { get; set; }

    public DateTime Timestamp { get; set; }

    public List<Message> Messages { get; set; }
}