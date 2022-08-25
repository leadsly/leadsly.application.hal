namespace Domain.Models.Requests
{
    public class ConnectionSentRequest
    {
        public string ServiceDiscoveryName { get; set; } = string.Empty;
        public string NamespaceName { get; set; } = string.Empty;
        public string RequestUrl { get; set; } = string.Empty;
        public string HalId { get; set; } = string.Empty;
        public string ProfileUrl { get; set; }
        public string Name { get; set; }
        public string CampaignId { get; set; }
        public long ConnectionSentTimestamp { get; set; }
    }
}
