namespace concierge_agent_api.Models
{
    public class EventWorkflowRequest
    {
        public string TMEmail { get; set; }
        public long TMAcctId { get; set; }

        public string TMEventId { get; set; }
        public bool HasParkingFlag { get; set; } // denotes whether the customer has parking and determines what message to send to the customer
    }
}
