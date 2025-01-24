using Newtonsoft.Json;

namespace concierge_agent_api.Models;

public record Session
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    [JsonProperty(PropertyName = "id")]
    public string Id { get; set; }

    public string Type { get; set; }

    public string SmsNumber { get; set; }

    public string SessionId { get; set; }

    public string UserId { get; set; }

    public string? Name { get; set; }

    public DateTime Timestamp { get; set; }

    [JsonIgnore]
    public List<Message> Messages { get; set; }

    public void AddMessage(Message message)
    {
        Messages.Add(message);
    }

    public void UpdateMessage(Message message)
    {
        var match = Messages.Single(m => m.Id == message.Id);
        var index = Messages.IndexOf(match);
        Messages[index] = message;
    }
}