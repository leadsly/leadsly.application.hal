using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.PhaseConsumers.ScanProspectsForRepliesHandlers
{
    public class ScanProspectsForRepliesConsumerCommand : IConsumeCommand
    {
        public ScanProspectsForRepliesConsumerCommand(string halId)
        {
            HalId = halId;
        }

        public string HalId { get; set; }
    }
}
