namespace concierge_agent_api.Models
{
    public class DimEventMaster
    {
        public long DimEventMasterId { get; set; }
        public long DimSeasonId { get; set; }
        public long DimSeasonMasterId { get; set; }
        public string EventDate { get; set; }
        public string TMEventId { get; set; }
        public string TMEventName { get; set; }
        public string TMEventNameLong { get; set; }
        public string TMTeam { get; set; }
        public string TMSeasonYear { get; set; }
        public string GatesOpen { get; set; }
        public string StartTime { get; set; }
        public string ActualStartTime { get; set; }
        public string HalfTimeStart { get; set; }
        public string HalfTimeEnd { get; set; }
        public string EndOfThirdQuarter { get; set; }
        public string EndTime { get; set; }
        public string LengthInMinutes { get; set; }
        public string OpponentName { get; set; }
        public string IsRoofOpen { get; set; }
        public string IsFullVenue { get; set; }
        public string EventSubType { get; set; }
        public string ReportedDistributed { get; set; }
        public string ReportedAttendance { get; set; }
        public string Win { get; set; }
        public string Loss { get; set; }
        public string Tie { get; set; }
        public string ScoreFinal { get; set; }
        public string OpponentScoreFinal { get; set; }
        public string EventType { get; set; }
        public string EventTypeSort { get; set; }
        public string SeasonYear { get; set; }
        public string Depot_EventMasterId { get; set; }
        public string OrgId { get; set; }
        public string EventTypeId { get; set; }
        public DateTime ETL_Create_DTM { get; set; }
        public DateTime ETL_Update_DTM { get; set; }
    }
}
