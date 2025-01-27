namespace concierge_agent_api.Models
{
    public class PointOfInterest
    {
        public PointOfInterestSummary Summary { get; set; }
        public List<Result> Results { get; set; }
    }
    public class PointOfInterestSummary
    {
        public string Query { get; set; }
        public string QueryType { get; set; }
        public int QueryTime { get; set; }
        public int NumResults { get; set; }
        public int Offset { get; set; }
        public int TotalResults { get; set; }
        public int FuzzyLevel { get; set; }
        public PointOfInterest GeoBias { get; set; }
    }
    public class MatchConfidence
    {
        public double Score { get; set; }
    }
    public class CategorySet
    {
        public int Id { get; set; }
    }
    public class ClassificationName
    {
        public string NameLocale { get; set; }
        public string Name { get; set; }
    }
    public class Classification
    {
        public string Code { get; set; }
        public List<ClassificationName> Names { get; set; }
    }
    public class Poi
    {
        public string Name { get; set; }
        public List<CategorySet> CategorySet { get; set; }
        public string Url { get; set; }
        public List<string> Categories { get; set; }
        public List<Classification> Classifications { get; set; }
    }
    public class PointOfInterestAddress
    {
        public string StreetNumber { get; set; }
        public string StreetName { get; set; }
        public string Municipality { get; set; }
        public string Neighbourhood { get; set; }
        public string CountrySecondarySubdivision { get; set; }
        public string CountrySubdivision { get; set; }
        public string CountrySubdivisionName { get; set; }
        public string CountrySubdivisionCode { get; set; }
        public string PostalCode { get; set; }
        public string ExtendedPostalCode { get; set; }
        public string CountryCode { get; set; }
        public string Country { get; set; }
        public string CountryCodeISO3 { get; set; }
        public string FreeformAddress { get; set; }
        public string LocalName { get; set; }
    }
    public class Position
    {
        public double Lat { get; set; }
        public double Lon { get; set; }
    }
    public class Viewport
    {
        public Position TopLeftPoint { get; set; }
        public Position BtmRightPoint { get; set; }
    }
    public class EntryPoint
    {
        public string Type { get; set; }
        public Position Position { get; set; }
    }
    public class Result
    {
        public string Type { get; set; }
        public string Id { get; set; }
        public double Score { get; set; }
        public double Dist { get; set; }
        public string Info { get; set; }
        public MatchConfidence MatchConfidence { get; set; }
        public Poi Poi { get; set; }
        public PointOfInterestAddress Address { get; set; }
        public Position Position { get; set; }
        public Viewport Viewport { get; set; }
        public List<EntryPoint> EntryPoints { get; set; }
    }
}
