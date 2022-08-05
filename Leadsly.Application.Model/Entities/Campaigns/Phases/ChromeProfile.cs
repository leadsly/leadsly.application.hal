using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.Entities.Campaigns.Phases
{
    public class ChromeProfile
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string ChromeProfileId { get; set; }

        public string Name { get; set; }
        public PhaseType CampaignPhaseType { get; set; }
    }
}
