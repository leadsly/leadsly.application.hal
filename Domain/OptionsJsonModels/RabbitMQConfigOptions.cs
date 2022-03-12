using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.OptionsJsonModels
{
    public class RabbitMQConfigOptions
    {
        public ConnectionFactoryConfigOptions ConnectionFactoryConfigOptions { get; set; }
        public ExchangeConfigOptions ExchangeConfigOptions { get; set; }
        public QueueConfigOptions QueueConfigOptions { get; set; }
    }

    public class ConnectionFactoryConfigOptions
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string HostName { get; set; }
        public int Port { get; set; }
        public string ClientProvidedName { get; set; }
    }

    public class ExchangeConfigOptions
    {
        public string Name { get; set; }
        public string ExchangeType { get; set; }
    }

    public class QueueConfigOptions
    {
        public bool AutoAcknowledge { get; set; }
    }
}
