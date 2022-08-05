using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.Entities.Campaigns
{
    public class CampaignProspectList
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string CampaignProspectListId { get; set; }
        public string PrimaryProspectListId { get; set; }

        public string ProspectListName { get; set; }
        public PrimaryProspectList PrimaryProspectList { get; set; }

        public List<SearchUrl> SearchUrls { get; set; }

        /// <summary>
        /// Represents the prospects for this campaign. Each campaign can create a new prospect list and use existing prospect list.
        /// If an existing prospect list is used, each campaign needs to have a way of tracking communication with each propsect on a
        /// per campaign level. Each new campaign will need to create its own prospect list as well as master list propsect list
        /// </summary>
        public ICollection<CampaignProspect> CampaignProspects { get; set; }
    }
}
