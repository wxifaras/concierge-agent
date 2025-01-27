using Newtonsoft.Json;

namespace concierge_agent_api.Models
{
    public class StreetAddress
    {
        public StreetAddressSummary Summary { get; set; }
        public List<AddressInfo> Addresses { get; set; }
    }
    public class StreetAddressSummary
    {
        public int QueryTime { get; set; }
        public int NumResults { get; set; }
    }

    public class AddressInfo
    {
        public StreetAddressInfo Address { get; set; }
        public string Position { get; set; }
        public string Id { get; set; }
    }

    public class StreetAddressInfo
    {
        public string BuildingNumber { get; set; }
        public string StreetNumber { get; set; }
        public List<string> RouteNumbers { get; set; }
        public string Street { get; set; }
        public string StreetName { get; set; }
        public string StreetNameAndNumber { get; set; }
        public string CountryCode { get; set; }
        public string CountrySubdivision { get; set; }
        public string CountrySecondarySubdivision { get; set; }
        public string Municipality { get; set; }
        public string PostalCode { get; set; }
        public string Neighbourhood { get; set; }
        public string Country { get; set; }
        public string CountryCodeISO3 { get; set; }
        public string FreeformAddress { get; set; }
        public BoundingBox BoundingBox { get; set; }
        public string CountrySubdivisionName { get; set; }
        public string CountrySubdivisionCode { get; set; }
        public string LocalName { get; set; }
    }

    public class BoundingBox
    {
        public string NorthEast { get; set; }
        public string SouthWest { get; set; }
        public string Entity { get; set; }
    }
}
