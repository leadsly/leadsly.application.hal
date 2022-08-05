using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.RabbitMQ
{
    public class QueueOptions
    {
        public string Name { get; set; }
        public bool AutoAcknowledge { get; set; }
    }
}
