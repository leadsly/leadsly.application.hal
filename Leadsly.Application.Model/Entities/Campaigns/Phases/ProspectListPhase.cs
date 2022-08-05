using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.Entities.Campaigns.Phases
{
    public class ProspectListPhase
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string ProspectListPhaseId { get; set; }
        public string CampaignId { get; set; }
        public PhaseType PhaseType { get; set; }
        public List<string> SearchUrls { get; set; }
        public bool Completed { get; set; }
        public long CompletedTimestamp { get; set; }        
        public Campaign Campaign { get; set; }
    }
}
