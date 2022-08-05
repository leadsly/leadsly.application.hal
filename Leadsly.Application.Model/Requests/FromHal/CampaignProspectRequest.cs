using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.Requests.FromHal
{
    [DataContract]
    public class CampaignProspectRequest : BaseHalRequest
    {
        [DataMember(Name = "ProfileUrl", IsRequired = true)]
        public string ProfileUrl { get; set; }
        [DataMember(Name = "Name", IsRequired = true)]
        public string Name { get; set; }
        [DataMember(Name = "CampaignId", IsRequired = true)]
        public string CampaignId { get; set; }

        [DataMember(Name = "ConnectionSentTimestamp", IsRequired = true)]
        public long ConnectionSentTimestamp { get; set; }
    }
}
