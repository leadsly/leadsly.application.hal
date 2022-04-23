using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.PhaseConsumers.FollowUpMessageHandlers
{
    public class FollowUpMessageConsumerCommand : ICommand
    {
        public FollowUpMessageConsumerCommand(string halId)
        {
            HalId = halId;
        }

        public string HalId { get; set; }
    }
}
