using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.Entities.Campaigns
{
    /// <summary>
    /// This is used when we only run ProspectListPhase and SendConnectionsPhase at the same time
    /// </summary>
    public class SearchUrlProgress
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string SearchUrlProgressId { get; set; }
        public string WindowHandleId { get; set; }
        public int LastPage { get; set; }
        public bool Exhausted { get; set; }
        public bool StartedCrawling { get; set; }
        public long LastActivityTimestamp { get; set; }
        public int TotalSearchResults { get; set; }
        public int LastProcessedProspect { get; set; }
        public string SearchUrl { get; set; }
        public string CampaignId { get; set; }
        public Campaign Campaign { get; set; }
    }
}
