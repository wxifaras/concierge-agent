namespace concierge_agent_api.Models
{
    public class LotLookup
    {
        public string lot_name { get; set; }
        public string actual_lot { get; set; }
        public DateTime first_event { get; set; }
        public DateTime last_event { get; set; }
        public DateTime ETL_Created_DTM { get; set; }
        public DateTime ETL_Updated_DTM { get; set; }
    }
}
