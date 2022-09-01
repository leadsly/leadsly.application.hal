using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Leadsly.Application.Model.Campaigns
{
    [DataContract]
    public class ScanProspectsForRepliesBody : PublishMessageBody
    {
        [DataMember]
        public string PageUrl { get; set; } = string.Empty;
        [DataMember]
        public IList<ContactedCampaignProspect> ContactedCampaignProspects { get; set; }
    }
}
