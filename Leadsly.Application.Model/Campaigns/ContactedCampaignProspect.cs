using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.Campaigns
{
    [DataContract]
    public class ContactedCampaignProspect
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string LastFollowUpMessageContent { get; set; }
        [DataMember]
        public string CampaignProspectId { get; set; }
        [DataMember]
        public string ProspectProfileUrl { get; set; }
    }
}
