namespace concierge_agent_api.Models
{
    public class LatLongLookUp
    {
        public LatLongSummary Summary { get; set; }
        public List<LatLongResult> Results { get; set; }
    }

    public class LatLongSummary
    {
        public string Query { get; set; }
        public string QueryType { get; set; }
        public int QueryTime { get; set; }
        public int NumResults { get; set; }
        public int Offset { get; set; }
        public int TotalResults { get; set; }
        public int FuzzyLevel { get; set; }
    }

    public class LatLongResult
    {
        public string Type { get; set; }
        public string Id { get; set; }
        public double Score { get; set; }
        public LatLongLookUpAddress Address { get; set; }
        public LatLongPosition Position { get; set; }
        public LatLongViewport Viewport { get; set; }
        public List<LatLongEntryPoint> EntryPoints { get; set; }
    }

    public class LatLongLookUpAddress
    {
        public string StreetNumber { get; set; }
        public string StreetName { get; set; }
        public string MunicipalitySubdivision { get; set; }
        public string Municipality { get; set; }
        public string CountrySecondarySubdivision { get; set; }
        public string CountryTertiarySubdivision { get; set; }
        public string CountrySubdivisionCode { get; set; }
        public string PostalCode { get; set; }
        public string ExtendedPostalCode { get; set; }
        public string CountryCode { get; set; }
        public string Country { get; set; }
        public string CodeISO3 { get; set; }
        public string FreeformAddress { get; set; }
        public string CountrySubdivisionName { get; set; }
    }
    public class LatLongPosition
    {
        public double Lat { get; set; }
        public double Lon { get; set; }
    }
    public class LatLongViewport
    {
        public Position TopLeftPoint { get; set; }
        public Position BtmRightPoint { get; set; }
    }

    public class LatLongEntryPoint
    {
        public string Type { get; set; }
        public LatLongPosition Position { get; set; }
    }



}
