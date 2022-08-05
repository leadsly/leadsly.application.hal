using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.Entities.Campaigns
{
    public class FollowUpMessage
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string FollowUpMessageId { get; set; }
        public string Content { get; set; }
        public int Order { get; set; }
        public FollowUpMessageDelay Delay { get; set; }
        public string CampaignId { get; set; }
        public Campaign Campaign { get; set; }
    }
}
