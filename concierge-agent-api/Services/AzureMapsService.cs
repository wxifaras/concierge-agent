using concierge_agent_api.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace concierge_agent_api.Services
{
    public interface IAzureMapsService
    {
        Task<string> GetAddressAsync(double latitude, double longitude);
        Task<string> GetDirectionsAsync(double startLatitude, double startLongitude, double endLatitude, double endLongitude);
        Task<(double, double)> GetPointOfInterestAsync(string businessName, double latitude, double longitude, int radius);
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

        public async Task<string> GetAddressAsync(double latitude, double longitude)
        {
            var query = $"https://atlas.microsoft.com/search/address/reverse/json?api-version=1.0&query={latitude},{longitude}&subscription-key={_subscriptionKey}";
            var response = await _client.GetAsync(query);
            response.EnsureSuccessStatusCode();
            var jsonString = await response.Content.ReadAsStringAsync();

            return jsonString;
        }

        public async Task<string> GetDirectionsAsync(double startLatitude, double startLongitude, double endLatitude, double endLongitude)
        {
            var query = $"https://atlas.microsoft.com/route/directions/json?api-version=1.0&query={startLatitude},{startLongitude}:{endLatitude},{endLongitude}&subscription-key={_subscriptionKey}";
            var response = await _client.GetAsync(query);
            response.EnsureSuccessStatusCode();
            var jsonString = await response.Content.ReadAsStringAsync();

            var jsonObject = JObject.Parse(jsonString);
            var summary = jsonObject["routes"]?[0]?["summary"];

            if (summary != null)
            {
                return summary.ToString();
            }

            throw new Exception("No summary found in the route.");
        }

        public async Task<(double, double)> GetPointOfInterestAsync(string businessName, double latitude, double longitude, int radius)
        {
            var query = $"https://atlas.microsoft.com/search/poi/json?api-version=1.0&query={businessName}&countrySet=US&lat={latitude}&lon={longitude}&radius={radius}&subscription-key={_subscriptionKey}";
            var response = await _client.GetAsync(query);
            response.EnsureSuccessStatusCode();
            var jsonString = await response.Content.ReadAsStringAsync();

            var jsonObject = JObject.Parse(jsonString);
            var firstResult = jsonObject["results"]?.FirstOrDefault();

            if (firstResult != null)
            {

                var latString = firstResult["entryPoints"][0]?["position"]["lat"].ToString();
                var lonString = firstResult["entryPoints"][0]?["position"]["lon"].ToString();

                if (double.TryParse(latString, out double lat) && double.TryParse(lonString, out double lon))
                {
                    return (lat, lon);
                }
                else
                {
                    throw new Exception("Failed to parse latitude or longitude.");
                }
            }

            throw new Exception("No point of interest found.");

        }
    }
}
