using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.Entities.Campaigns
{
    /// <summary>
    /// This is used when we run ProspectListPhase first in totality and THEN execute SendConnectionsPhase
    /// </summary>
    public class SearchUrlDetails
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string SearchUrlDetailsId { get; set; }
        public bool FinishedCrawling { get; set; }
        public bool StartedCrawling { get; set; }
        public string WindowHandleId { get; set; }
        public string OriginalUrl { get; set; }
        public string CurrentUrl { get; set; }        
        public long LastActivityTimestamp { get; set; }
        public string CampaignId { get; set; }
        public Campaign Campaign { get; set; }
    }
}
