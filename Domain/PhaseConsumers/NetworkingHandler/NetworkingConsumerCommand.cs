using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.PhaseConsumers.NetworkingHandler
{
    public class NetworkingConsumerCommand : IConsumeCommand
    {
        public NetworkingConsumerCommand(string halId)
        {
            HalId = halId;
        }
        public string HalId { get; set; }
    }
}
