using Newtonsoft.Json;

namespace concierge_agent_api.Models;

public class LotLocation
{
    public string actual_lot { get; set; }
    public double lat { get; set; }
    [JsonProperty("long")]
    public double longitude { get; set; }
    public string locationType { get; set; }
}
