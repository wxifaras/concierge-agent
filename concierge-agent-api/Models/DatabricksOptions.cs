using System.ComponentModel.DataAnnotations;

namespace concierge_agent_api.Models;

public class DatabricksOptions
{
    public const string AzureDatabricks = "DatabricksOptions";

    [Required]
    public string DatabricksInstance { get; set; }       
    [Required]
    public string Token { get; set; }
    [Required]
    public string WarehouseId { get; set; }
}
