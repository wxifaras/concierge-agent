using System.ComponentModel.DataAnnotations;

namespace concierge_agent_api.Models
{
    public class AzureMapsOptions
    {
        public const string AzureMaps = "AzureMapsOptions";
        
        public string SubscriptionKey { get; set; }

    }
}
