using Newtonsoft.Json;

namespace concierge_agent_api.Models;

public class LotLocation
{
    public string actual_lot { get; set; }
    public double lat { get; set; }
    [JsonProperty("long")]
    public double longitude { get; set; }
    public string locationType { get; set; }
    public int lot_price {  get; set; }
    public string TMEventId { get; set; }
}

public class EnrichedLotLocation
{
    public string lot_lat { get; set; }
    public string lot_long { get; set; }
    public string actual_lot { get; set; }
    public string location_type { get; set; }
    public string distance_to_stadium_in_meters { get; set; }
}