﻿using Domain.Models.ProspectList;
using System.Collections.Generic;

namespace Domain.Models.Requests.ProspectList
{
    public class CollectedProspectsRequest
    {
        public string ServiceDiscoveryName { get; set; } = string.Empty;
        public string NamespaceName { get; set; } = string.Empty;
        public string RequestUrl { get; set; } = string.Empty;
        public string CampaignProspectListId { get; set; }
        public string PrimaryProspectListId { get; set; }
        public string CampaignId { get; set; }
        public string SocialAccountId { get; set; }
        public IList<PersistPrimaryProspectModel> Items { get; set; }
    }
}
