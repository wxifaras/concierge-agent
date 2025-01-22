namespace concierge_agent_api.Models
{
    public class Customer
    {
        public string TMEmail { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public long TMAcctId { get; set; }
        public long EpsilonCustomerKey { get; set; }
        public string WicketId { get; set; }
        public string ConstellationId { get; set; }
        public string AifiCustomerId { get; set; }
        public string TMCellPhone { get; set; }
        public string TMMAPhone { get; set; }
        public string EpsilonSMSNumber { get; set; }
        public string hasBioPhoto { get; set; }
        public string bioWebOnboarded { get; set; }
        public string bioAppOnboarded { get; set; }
        public string bioDateJoined { get; set; }
        public string bioLastUpdated { get; set; }
        public string geniusCheckoutStoredCards { get; set; }
        public string storedCards { get; set; }
        public string signUpMethod { get; set; }
        public bool CurrentFalconsSTM { get; set; }
        public bool CurrentUnitedSTM { get; set; }
        public bool emailOptIn_AF { get; set; }
        public bool emailOptIn_AU { get; set; }
        public bool emailOptIn_MBS { get; set; }
        public bool smsFalconsOptInFlag { get; set; }
        public bool smsUniteOptInFlag { get; set; }
        public bool smsMBSOptInFlag { get; set; }
    }
}
