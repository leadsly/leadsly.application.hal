using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.Entities.Campaigns
{
    public class CampaignWarmUp
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string CampaignWarmUpId { get; set; }
        public int DailyLimit { get; set; }
        public long StartDateTimestamp { get; set; }
        public string CampaignId { get; set; }
        public Campaign Campaign { get; set; }
    }
}
