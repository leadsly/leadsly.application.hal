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
    public class ScanProspectsForRepliesBody : PublishMessageBody
    {
        [DataMember(Name = "PageUrl")]
        public string PageUrl { get; set; }

        [DataMember(Name = "ContactedCampaignProspects", IsRequired = false)]
        public IList<ContactedCampaignProspect> ContactedCampaignProspects { get; set; }
    }
}
