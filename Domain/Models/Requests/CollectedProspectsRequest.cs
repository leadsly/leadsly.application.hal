using System.Collections.Generic;

namespace Domain.Models.Requests
{
    public class CollectedProspectsRequest
    {
        public string ServiceDiscoveryName { get; set; } = string.Empty;
        public string NamespaceName { get; set; } = string.Empty;
        public string RequestUrl { get; set; } = string.Empty;
        public string HalId { get; set; } = string.Empty;
        public string UserId { get; set; }
        public string CampaignId { get; set; }
        public string PrimaryProspectListId { get; set; }
        public string CampaignProspectListId { get; set; }
        public IList<PersistPrimaryProspectRequest> Prospects { get; set; }
        public string AccessToken { get; set; }
    }
}
