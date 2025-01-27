namespace concierge_agent_api.Models
{
    public class RouteSummary
    {
        public string FormatVersion { get; set; }
        public List<Route> Routes { get; set; }
        public string mapLink { get; set; }
    }
    public class Route
    {
        public Summary Summary { get; set; }
        public List<Leg> Legs { get; set; }
        public List<Section> Sections { get; set; }
    }
    public class Summary
    {
        public int LengthInMeters { get; set; }
        public int TravelTimeInSeconds { get; set; }
        public int TrafficDelayInSeconds { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
    }
    public class Leg
    {
        public Summary Summary { get; set; }
        public List<Point> Points { get; set; }
    }
    public class Point
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
    public class RouteSummarySection
    {
        public int StartPointIndex { get; set; }
        public int EndPointIndex { get; set; }
        public string SectionType { get; set; }
        public string TravelMode { get; set; }
    }

}
