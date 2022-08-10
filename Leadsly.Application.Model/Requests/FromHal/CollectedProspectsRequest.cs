using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Leadsly.Application.Model.Requests.FromHal
{
    [DataContract]
    public class CollectedProspectsRequest : BaseHalRequest
    {
        [DataMember(Name = "UserId", IsRequired = true)]
        public string UserId { get; set; }

        [DataMember(Name = "CampaignId", IsRequired = true)]
        public string CampaignId { get; set; }

        [DataMember(Name = "PrimaryProspectListId", IsRequired = true)]
        public string PrimaryProspectListId { get; set; }

        [DataMember(Name = "CampaignProspectListId", IsRequired = true)]
        public string CampaignProspectListId { get; set; }

        [DataMember(Name = "Prospects", IsRequired = true)]
        public IList<PrimaryProspectRequest> Prospects { get; set; }

        [DataMember(Name = "AccessToken", IsRequired = true)]
        public string AccessToken { get; set; }
    }
}
