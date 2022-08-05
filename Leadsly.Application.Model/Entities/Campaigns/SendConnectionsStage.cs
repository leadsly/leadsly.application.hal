using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.Entities.Campaigns
{
    /// <summary>
    /// Represents the time in the day when to send connections and how many
    /// </summary>
    public class SendConnectionsStage
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string SendConnectionsStageId { get; set; }
        public string CampaignId { get; set; }        
        public string StartTime { get; set; }        
        public int Order { get; set; }
        public int NumOfConnections { get; set; }
        public Campaign Campaign { get; set; }
    }
}
