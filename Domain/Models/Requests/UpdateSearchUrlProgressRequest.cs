namespace Domain.Models.Requests
{
    public class UpdateSearchUrlProgressRequest
    {
        public string ServiceDiscoveryName { get; set; } = string.Empty;
        public string NamespaceName { get; set; } = string.Empty;
        public string RequestUrl { get; set; } = string.Empty;
        public string HalId { get; set; } = string.Empty;
        public string SearchUrlProgressId { get; set; }
        public string WindowHandleId { get; set; }
        public int LastPage { get; set; }
        public int LastProcessedProspect { get; set; }
        public string SearchUrl { get; set; }
        public bool StartedCrawling { get; set; }
        public int TotalSearchResults { get; set; }
        public bool Exhausted { get; set; }
        public string CampaignId { get; set; }
        public long LastActivityTimestamp { get; set; }
    }
}
