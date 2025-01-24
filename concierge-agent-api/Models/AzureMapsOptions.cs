using System.ComponentModel.DataAnnotations;

namespace concierge_agent_api.Models;

public class AzureMapsOptions
{
    public const string AzureMaps = "AzureMapsOptions";

    [Required]
    public string SubscriptionKey { get; set; }
}
