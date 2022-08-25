using System.Collections.Generic;

namespace Domain.Models.Requests
{
    public class CampaignProspectListRequest
    {
        public IList<ConnectionSentRequest> CampaignProspects { get; set; }
        public string CampaignId { get; set; }
        public string UserId { get; set; }
        public string ServiceDiscoveryName { get; set; } = string.Empty;
        public string NamespaceName { get; set; } = string.Empty;
        public string RequestUrl { get; set; } = string.Empty;
        public string HalId { get; set; } = string.Empty;
    }
}
