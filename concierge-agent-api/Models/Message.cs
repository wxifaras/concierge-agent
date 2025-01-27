using Newtonsoft.Json;

namespace concierge_agent_api.Models;

public record Message
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    [JsonProperty(PropertyName = "id")]
    public string Id { get; set; }

    public string Type { get; set; } // will map to SK AuthorRole

    //public string SessionId { get; set; }

    public DateTime TimeStamp { get; set; }

    public string Prompt { get; set; }

    public string Sender { get; set; }

    //public string Completion { get; set; }
}