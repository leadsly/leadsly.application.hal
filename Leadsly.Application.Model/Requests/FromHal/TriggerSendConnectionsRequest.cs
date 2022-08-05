using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.Requests.FromHal
{
    [DataContract]
    public class TriggerSendConnectionsRequest : BaseHalRequest
    {
        [DataMember]
        public string CampaignId { get; set; }
        [DataMember]
        public string UserId { get; set; }
    }
}
