using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.PhaseHandlers.NetworkingConnectionsHandler
{
    public class ProspectListCommand : ICommand
    {
        public ProspectListCommand(IModel channel, BasicDeliverEventArgs eventArgs)
        {
            Channel = channel;
            EventArgs = eventArgs;
        }

        public IModel Channel { get; set; }
        public BasicDeliverEventArgs EventArgs { get; set; }
    }
}
