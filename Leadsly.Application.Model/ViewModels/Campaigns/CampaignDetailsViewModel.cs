using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.ViewModels.Campaigns
{
    [DataContract]
    public class CampaignDetailsViewModel
    {
        [DataMember(Name = "name", IsRequired = true)]
        public string Name { get; set; }

        [DataMember(Name = "dailyInviteLimit", IsRequired = true)]
        public int DailyInviteLimit { get; set; }

        [DataMember(Name = "warmUp", IsRequired = true)]
        public bool WarmUp { get; set; }

        [DataMember(Name = "startTimestamp", IsRequired = false)]
        public long StartTimestamp { get; set; }

        [DataMember(Name = "endTimestamp", IsRequired = false)]
        public long EndTimestamp { get; set; }

        [DataMember(Name = "campaignType", IsRequired = true)]
        public CampaignTypeEnum CampaignType { get; set; }

        [DataMember(Name = "primaryProspectList", IsRequired = false)]
        public ProspectListViewModel PrimaryProspectList { get; set; }
        
    }
}
