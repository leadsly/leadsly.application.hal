using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.Entities.Campaigns
{
    public class FollowUpMessageDelay
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string FollowUpMessageDelayId { get; set; }
        public string FollowUpMessageId { get; set; }
        public long Value { get; set; }
        public string Unit { get; set; }
        public FollowUpMessage FollowUpMessage { get; set; }
    }
}
