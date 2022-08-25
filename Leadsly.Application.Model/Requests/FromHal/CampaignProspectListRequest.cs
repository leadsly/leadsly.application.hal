using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Leadsly.Application.Model.Requests.FromHal
{
    [DataContract]
    public class CampaignProspectListRequest : BaseHalRequest
    {
        [DataMember(Name = "CampaignProspects", IsRequired = true)]
        public IList<CampaignProspectRequest> CampaignProspects { get; set; }

        [DataMember(Name = "CampaignId", IsRequired = true)]
        public string CampaignId { get; set; }

        [DataMember(Name = "UserId", IsRequired = true)]
        public string UserId { get; set; }
    }
}
