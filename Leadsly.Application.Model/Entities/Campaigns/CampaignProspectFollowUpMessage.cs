using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.Entities.Campaigns
{
    public class CampaignProspectFollowUpMessage
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string CampaignProspectFollowUpMessageId { get; set; }
        public string CampaignProspectId { get; set; }
        public CampaignProspect CampaignProspect { get; set; }
        public int Order { get; set; }
        public string Content { get; set; }
    }
}
