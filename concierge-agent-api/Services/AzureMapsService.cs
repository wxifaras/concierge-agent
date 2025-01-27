using concierge_agent_api.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace concierge_agent_api.Services
{
    // travel mode options for the map service: https://learn.microsoft.com/en-us/rest/api/maps/route/get-route-directions?view=rest-maps-2024-04-01&tabs=HTTP#travelmode
    public enum TravelMode
    {
        bicycle,
        bus,
        car,
        motorcycle,
        pedestrian,
        taxi,
        truck,
        van
    }

    public interface IAzureMapsService
    {
        Task<StreetAddress> GetAddressAsync(double latitude, double longitude);
        Task<RouteSummary> GetDirectionsAsync(double startLatitude, double startLongitude, double endLatitude, double endLongitude, TravelMode travelMode);
        Task<int> GetDistanceAsync(double startLatitude, double startLongitude, double endLatitude, double endLongitude, TravelMode travelMode);
        Task<PointOfInterest> GetPointOfInterestAsync(string businessName, double latitude, double longitude, int radius);
    }

    public class AzureMapsService : IAzureMapsService
    {
        private static readonly HttpClient _client;
        private readonly string _subscriptionKey;

        static AzureMapsService()
        {
            _client = new HttpClient();
        }
        public AzureMapsService(IOptions<AzureMapsOptions> options)
        {
            _subscriptionKey = options.Value.SubscriptionKey;
        }

        //Get a street address and location info from latitude and longitude coordinates.
        //https://learn.microsoft.com/en-us/rest/api/maps/search/get-search-address-reverse?view=rest-maps-1.0&preserve-view=true&tabs=HTTP#searches-addresses-for-coordinates-37.337,-121.89
        //json response//
        //{"summary":{"queryTime":8,"numResults":1},"addresses":[{"address":{"buildingNumber":"35","streetNumber":"35","routeNumbers":
        //["US-19 North","US-19 South","US-29 North","US-29 South","US-41 N","US-41 South","Georgia 3"],"street":"Northside Drive Northwest","streetName":
        //"Northside Drive Northwest","streetNameAndNumber":"35 Northside Drive Northwest","countryCode":"US","countrySubdivision":"GA","countrySecondarySubdivision":
        //"Fulton","municipality":"Atlanta","postalCode":"30313","neighbourhood":"Downtown Atlanta","country":"United States","countryCodeISO3":"USA","freeformAddress"
        //:"35 Northside Drive Northwest, Atlanta, GA 30313","boundingBox":{"northEast":"33.755899,-84.402792","southWest":"33.755450,-84.402798","entity":"position"},
        //"countrySubdivisionName":"Georgia","countrySubdivisionCode":"GA","localName":"Atlanta"},"position":"33.755562,-84.402794","id":"KMQAsoEXTfDxrsjQaPl_eQ"}]}
        public async Task<StreetAddress> GetAddressAsync(double latitude, double longitude)
        {
            var query = $"https://atlas.microsoft.com/search/address/reverse/json?api-version=1.0&query={latitude},{longitude}&subscription-key={_subscriptionKey}";
            var response = await _client.GetAsync(query);
            response.EnsureSuccessStatusCode();
            var jsonString = await response.Content.ReadAsStringAsync();
            var streetAddress = JsonConvert.DeserializeObject<StreetAddress>(jsonString);

            return streetAddress;
        }

        //Gets the route directions
        //https://learn.microsoft.com/en-us/rest/api/maps/route/get-route-directions?view=rest-maps-2024-04-01&tabs=HTTP#successfully-retrieve-a-route-between-an-origin-and-a-destination
        //json response//
        //{"formatVersion":"0.0.12","routes":[{"summary":{"lengthInMeters":1367,"travelTimeInSeconds":984,"trafficDelayInSeconds":0,"trafficLengthInMeters":0,"departureTime"
        //:"2025-01-25T21:24:59-05:00","arrivalTime":"2025-01-25T21:41:22-05:00"},"legs":[{"summary":{"lengthInMeters":1367,"travelTimeInSeconds":984,"trafficDelayInSeconds"
        //:0,"trafficLengthInMeters":0,"departureTime":"2025-01-25T21:24:59-05:00","arrivalTime":"2025-01-25T21:41:22-05:00"},"points":[{"latitude":33.7546,"longitude":-84.39498}
        //,{"latitude":33.75483,"longitude":-84.39538},{"latitude":33.75496,"longitude":-84.39553},{"latitude":33.75492,"longitude":-84.39563},{"latitude":33.75485,"longitude":-84.39573}
        //,{"latitude":33.75473,"longitude":-84.39586},{"latitude":33.75461,"longitude":-84.39597},{"latitude":33.75447,"longitude":-84.39608},{"latitude":33.75448,"longitude":-84.39618}
        //,{"latitude":33.75451,"longitude":-84.3963},{"latitude":33.75454,"longitude":-84.39639},{"latitude":33.75463,"longitude":-84.39662},{"latitude":33.75465,"longitude":-84.39683}
        //,{"latitude":33.75466,"longitude":-84.39697},{"latitude":33.75467,"longitude":-84.39705},{"latitude":33.75467,"longitude":-84.39714},{"latitude":33.7559,"longitude":-84.39727}
        //,{"latitude":33.75605,"longitude":-84.39731},{"latitude":33.75599,"longitude":-84.3974},{"latitude":33.75594,"longitude":-84.3975},{"latitude":33.7559,"longitude":-84.3976}
        //,{"latitude":33.75584,"longitude":-84.39768},{"latitude":33.75572,"longitude":-84.39786},{"latitude":33.75566,"longitude":-84.39793},{"latitude":33.75553,"longitude":-84.39807}
        //,{"latitude":33.75539,"longitude":-84.39818},{"latitude":33.75536,"longitude":-84.39821},{"latitude":33.7553,"longitude":-84.39825},{"latitude":33.75526,"longitude":-84.39828}
        //,{"latitude":33.75521,"longitude":-84.39832},{"latitude":33.75514,"longitude":-84.39836},{"latitude":33.75499,"longitude":-84.39845},{"latitude":33.75494,"longitude":-84.39847}
        //,{"latitude":33.75454,"longitude":-84.3986},{"latitude":33.75455,"longitude":-84.39899},{"latitude":33.75455,"longitude":-84.39909},{"latitude":33.75455,"longitude":-84.39925}
        //,{"latitude":33.75448,"longitude":-84.39955},{"latitude":33.75442,"longitude":-84.39972},{"latitude":33.75416,"longitude":-84.40018},{"latitude":33.75404,"longitude":-84.40037}
        //,{"latitude":33.754,"longitude":-84.40047},{"latitude":33.75397,"longitude":-84.40052},{"latitude":33.75385,"longitude":-84.40077},{"latitude":33.75385,"longitude":-84.40151}
        //,{"latitude":33.75386,"longitude":-84.40215},{"latitude":33.75387,"longitude":-84.40257},{"latitude":33.75387,"longitude":-84.40281},{"latitude":33.75407,"longitude":-84.4028}
        //,{"latitude":33.75435,"longitude":-84.4028},{"latitude":33.75445,"longitude":-84.4028},{"latitude":33.75454,"longitude":-84.4028},{"latitude":33.75465,"longitude":-84.4028}
        //,{"latitude":33.75478,"longitude":-84.4028},{"latitude":33.75496,"longitude":-84.40281},{"latitude":33.75507,"longitude":-84.4028},{"latitude":33.75536,"longitude":-84.4028}
        //,{"latitude":33.75545,"longitude":-84.4028},{"latitude":33.7559,"longitude":-84.40279},{"latitude":33.756,"longitude":-84.40279},{"latitude":33.75613,"longitude":-84.40279}
        //,{"latitude":33.75633,"longitude":-84.40279},{"latitude":33.75635,"longitude":-84.40237},{"latitude":33.75641,"longitude":-84.4022},{"latitude":33.75649,"longitude":-84.40207}]}],
        //"sections":[{"startPointIndex":0,"endPointIndex":64,"sectionType":"TRAVEL_MODE","travelMode":"pedestrian"}]}]}
        public async Task<RouteSummary> GetDirectionsAsync(double startLatitude, double startLongitude, double endLatitude, double endLongitude, TravelMode travelMode)
        {
            string strTravelMode = travelMode.ToString().ToLower();

            var query = $"https://atlas.microsoft.com/route/directions/json?api-version=1.0&query={startLatitude},{startLongitude}:{endLatitude},{endLongitude}&travelMode={strTravelMode}&subscription-key={_subscriptionKey}";
            var response = await _client.GetAsync(query);
            response.EnsureSuccessStatusCode();
            var jsonString = await response.Content.ReadAsStringAsync();
            var routeSummary = JsonConvert.DeserializeObject<RouteSummary>(jsonString);

            routeSummary.mapLink = $"https://www.bing.com/maps?rtp=~pos.{startLatitude}_{startLongitude}~pos.{endLatitude}_{endLongitude}";

            return routeSummary;
        }

        //This method returns just the distance in meters. 
        //https://learn.microsoft.com/en-us/rest/api/maps/route/get-route-directions?view=rest-maps-2024-04-01&tabs=HTTP#successfully-retrieve-a-route-between-an-origin-and-a-destination
        //the json response is the same as GetDirectionsAsync. This method is just returning the LengthInMeters
        public async Task<int> GetDistanceAsync(double startLatitude, double startLongitude, double endLatitude, double endLongitude, TravelMode travelMode)
        {
            string strTravelMode = travelMode.ToString().ToLower();

            var query = $"https://atlas.microsoft.com/route/directions/json?api-version=1.0&query={startLatitude},{startLongitude}:{endLatitude},{endLongitude}&travelMode={strTravelMode}&subscription-key={_subscriptionKey}";
            var response = await _client.GetAsync(query);
            response.EnsureSuccessStatusCode();
            var jsonString = await response.Content.ReadAsStringAsync();
            var lengthInMeters = JsonConvert.DeserializeObject<RouteSummary>(jsonString);

            return lengthInMeters.Routes[0].Summary.LengthInMeters;
        }

        //Gets a point of interest- for example a business such as "Wild Leap"
        //we can use this to get the lattitude and longitude for the business. There may be multiple places returned.
        //The lat and long in the query is the center point of the radius. This could be set to stadium lat/lon and have a 20mile radius.
        //We want the lat/long at the end of the json response ****"entryPoints":[{"type":"main","position":{"lat":33.75113,"lon":-84.39665}}]}]}*****
        //https://learn.microsoft.com/en-us/rest/api/maps/search/get-search-poi?view=rest-maps-1.0&tabs=HTTP#search-for-juice-bars-within-5-miles-of-seattle-downtown-and-limit-the-response-to-5-results
        //json response//
        //{"summary":{"query":"wild leap","queryType":"NON_NEAR","queryTime":35,"numResults":1,"offset":0,"totalResults":1,"fuzzyLevel":1,"geoBias":{"lat":33.75528,"lon":-84.40083}}
        //,"results":[{"type":"POI","id":"_FJhFIzSKXMDEdwidloZTA","score":5.7633221922,"dist":580.397921,"info":"search:ta:840137000063950-US","matchConfidence":{"score":1},"poi":
        //{"name":"Wild Leap Atlanta","categorySet":[{"id":9379004}],"url":"wildleap.com/atlanta","categories":["bar","nightlife"],"classifications":[{"code":"NIGHTLIFE","names":
        //[{"nameLocale":"en-US","name":"bar"},{"nameLocale":"en-US","name":"nightlife"}]}]},"address":{"streetNumber":"125","streetName":"Ted Turner Drive Southwest","municipality"
        //:"Atlanta","neighbourhood":"CastleberryHill","countrySecondarySubdivision":"Fulton","countrySubdivision":"GA","countrySubdivisionName":"Georgia","countrySubdivisionCode"
        //:"GA","postalCode":"30303","extendedPostalCode":"30303-3704","countryCode":"US","country":"United States","countryCodeISO3":"USA","freeformAddress":"125 Ted Turner Drive Southwest,
        //Atlanta, GA 30303","localName":"Atlanta"},"position":{"lat":33.751242,"lon":-84.396852},"viewport":{"topLeftPoint":{"lat":33.75214,"lon":-84.39793},"btmRightPoint":
        //{"lat":33.75034,"lon":-84.39577}},"entryPoints":[{"type":"main","position":{"lat":33.75113,"lon":-84.39665}}]}]}
        public async Task<PointOfInterest> GetPointOfInterestAsync(string businessName, double latitude, double longitude, int radius)
        {
            var query = $"https://atlas.microsoft.com/search/poi/json?api-version=1.0&query={businessName}&countrySet=US&lat={latitude}&lon={longitude}&radius={radius}&subscription-key={_subscriptionKey}";
            var response = await _client.GetAsync(query);
            response.EnsureSuccessStatusCode();
            var jsonString = await response.Content.ReadAsStringAsync();
            var pointofInterest = JsonConvert.DeserializeObject<PointOfInterest>(jsonString);

            return pointofInterest;
        }
    }
}