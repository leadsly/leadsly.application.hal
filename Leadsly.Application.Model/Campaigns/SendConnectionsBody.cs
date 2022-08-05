using Leadsly.Application.Model.Entities.Campaigns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.Campaigns
{
    [DataContract]
    public class SendConnectionsBody : PublishMessageBody
    {
        [DataMember(Name = "SendConnectionsStage", IsRequired = true)]
        public SendConnectionsStageBody SendConnectionsStage { get; set; }

        [DataMember(Name = "DailyLimit", IsRequired = true)]
        public int DailyLimit { get; set; }

        [DataMember(Name = "StartDateTimestamp", IsRequired = true)]
        public long StartDateTimestamp { get; set; }

        [DataMember(Name = "CampaignId", IsRequired = true)]
        public string CampaignId { get; set; }

    }
}
