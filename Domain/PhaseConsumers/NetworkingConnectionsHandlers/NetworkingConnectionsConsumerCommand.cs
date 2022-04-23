using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.PhaseConsumers.NetworkingConnectionsHandlers
{
    public class NetworkingConnectionsConsumerCommand : ICommand
    {
        public NetworkingConnectionsConsumerCommand(string halId)
        {
            HalId = halId;
        }

        public string HalId { get; set; }
    }
}
