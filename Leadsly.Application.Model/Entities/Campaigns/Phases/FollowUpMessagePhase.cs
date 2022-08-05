using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.Entities.Campaigns.Phases
{
    public class FollowUpMessagePhase
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string FollowUpMessagePhaseId { get; set; }
        public string CampaignId { get; set; }
        public Campaign Campaign { get; set; }
        public string PageUrl { get; set; } = "https://www.linkedin.com/messaging/";
        public PhaseType PhaseType { get; set; }

    }
}
