using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.RabbitMQ
{
    public class ConnectionFactoryOptions
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string HostName { get; set; }
        public int Port { get; set; }
        public string ClientProvidedName { get; set; }
        public string VirtualHost { get; set; }
    }
}
