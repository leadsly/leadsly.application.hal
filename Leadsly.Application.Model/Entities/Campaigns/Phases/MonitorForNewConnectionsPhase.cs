using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.Entities.Campaigns.Phases
{
    public class MonitorForNewConnectionsPhase
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string MonitorForNewConnectionsPhaseId { get; set; }
        public string PageUrl { get; set; } = "https://www.linkedin.com/mynetwork/invite-connect/connections/";
        public PhaseType PhaseType { get; set; }
        public string SocialAccountId { get; set; }
        public SocialAccount SocialAccount { get; set; }
    }
}
