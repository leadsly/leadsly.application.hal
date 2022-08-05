using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.RabbitMQ
{
    public class RabbitMQOptions
    {
        public string RoutingKey { get; set; }
        public ConnectionFactoryOptions ConnectionFactoryOptions { get; set; }
        public ExchangeOptions ExchangeOptions { get; set; }
        public QueueOptions QueueConfigOptions { get; set; }
    }
}
