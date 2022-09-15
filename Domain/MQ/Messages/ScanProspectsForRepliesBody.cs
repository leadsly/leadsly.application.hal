using Leadsly.Application.Model.Campaigns;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Domain.MQ.Messages
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
