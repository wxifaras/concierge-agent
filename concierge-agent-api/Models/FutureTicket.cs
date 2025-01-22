namespace concierge_agent_api.Models
{
    public class FutureTicket
    {
        public string emailaddress { get; set; }
        public long CustomerKey { get; set; }
        public long FactTicketInventoryId { get; set; }
        public long TMEventId { get; set; }
        public string EventName { get; set; }
        public DateTime EventDate { get; set; }
        public string EventType { get; set; }
        public string EventSubType { get; set; }
        public string IsFalcons { get; set; }
        public string IsUnited { get; set; }
        public string IsPreseason { get; set; }
        public string IsFullVenue { get; set; }
        public long SeasonYear { get; set; }
        public long SeasonWeek { get; set; }
        public long SeasonWeekNoPreseason { get; set; }
        public string SectionName { get; set; }
        public string SectionClass { get; set; }
        public string SectionClassGroup { get; set; }
        public bool IsSold { get; set; }
        public long numSeats { get; set; }
        public DateTime OriginalPurchaseDate { get; set; }
        public string OriginalTicketStatus { get; set; }
        public long FinalAcctId { get; set; }
        public string finalEmail { get; set; }
        public string PriceCode { get; set; }
        public string PriceCodeDesc { get; set; }
        public string PC1 { get; set; }
        public string PC2 { get; set; }
        public string PC3 { get; set; }
        public string PC4 { get; set; }
        public double SumPurchasePrice { get; set; }
        public string FinalSalesChannel { get; set; }
        public string FinalActivityName { get; set; }
        public double TotalSecondaryPurchasePrice { get; set; }
        public string Tier { get; set; }
        public long TierPriority { get; set; }
    }
}
