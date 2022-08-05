using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.Requests.FromHal
{
    [DataContract]
    public class FollowUpMessageSentRequest : BaseHalRequest
    {
        [DataMember]
        public string CampaignProspectId { get; set; }
        [DataMember]
        public string ProspectName { get; set; }
        [DataMember]
        public int MessageOrderNum { get; set; }
        [DataMember]
        public long MessageSentTimestamp { get; set; }

    }
}
