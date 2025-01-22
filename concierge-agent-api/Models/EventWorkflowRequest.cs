using System.ComponentModel.DataAnnotations;

namespace concierge_agent_api.Models
{
    public class EventWorkflowRequest
    {
        [Required]
        public string TMEmail { get; set; }

        [Required]
        public long TMAcctId { get; set; }

        [Required]
        public string TMEventId { get; set; }

        public bool? HasParkingFlag { get; set; } // denotes whether the customer has parking and determines what message to send to the customer
    }
}
