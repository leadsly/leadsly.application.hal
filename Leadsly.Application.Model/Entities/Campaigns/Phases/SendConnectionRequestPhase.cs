using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.Entities.Campaigns.Phases
{
    public class SendConnectionRequestPhase
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string SendConnectionRequestPhaseId { get; set; }
        public string CampaignId { get; set; }
        public PhaseType PhaseType { get; set; }
        public Campaign Campaign { get; set; }

    }
}
