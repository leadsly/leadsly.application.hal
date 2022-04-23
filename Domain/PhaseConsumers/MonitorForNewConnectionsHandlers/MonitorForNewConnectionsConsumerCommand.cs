using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.PhaseConsumers.MonitorForNewConnectionsHandlers
{
    public class MonitorForNewConnectionsConsumerCommand : ICommand
    {
        public MonitorForNewConnectionsConsumerCommand(string halId)
        {
            HalId = halId;
        }
        public string HalId { get; set; }
    }
}
