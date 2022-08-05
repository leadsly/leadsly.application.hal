using Leadsly.Application.Model.Campaigns;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.RabbitMQ
{
    public class RabbitMQMessageProperties
    {
        public IModel Channel { get; set; }
        public BasicDeliverEventArgs BasicDeliveryEventArgs { get; set; }

        public Action BasicAck { get; set; }
        public Action BasicNack { get; set; }
        public SendConnectionsBody SendConnectionsBody { get; set; }

    }
}
