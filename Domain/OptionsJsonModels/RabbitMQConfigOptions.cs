namespace Domain.OptionsJsonModels
{
    public class RabbitMQConfigOptions
    {
        public RoutingKey RoutingKey { get; set; }
        public ConnectionFactoryConfigOptions ConnectionFactoryConfigOptions { get; set; }
        public ExchangeConfigOptions ExchangeConfigOptions { get; set; }
        public QueueConfigOptions QueueConfigOptions { get; set; }
    }

    public class RoutingKey
    {
        public string AppServer { get; set; }
        public string Hal { get; set; }
    }

    public class ConnectionFactoryConfigOptions
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string HostName { get; set; }
        public int Port { get; set; }
        public string VirtualHost { get; set; }
        public ClientProvidedName ClientProvidedName { get; set; }
        public Ssl Ssl { get; set; }
    }

    public class ClientProvidedName
    {
        public string AppServer { get; set; }
        public string Hal { get; set; }
    }

    public class Ssl
    {
        public bool Enabled { get; set; }
        public string ServerName { get; set; }
    }

    public class ExchangeConfigOptions
    {
        public AppServerConfigOptions AppServer { get; set; }
        public HalConfigOptions Hal { get; set; }
        public class AppServerConfigOptions
        {
            public string Name { get; set; }
            public string ExchangeType { get; set; }
        }
        public class HalConfigOptions
        {
            public string Name { get; set; }
            public string ExchangeType { get; set; }
        }
    }

    public class QueueConfigOptions
    {
        public AppServerConfigOptions AppServer { get; set; }
        public HalConfigOptions Hal { get; set; }
        public class AppServerConfigOptions
        {
            public string Name { get; set; }
            public bool AutoAcknowledge { get; set; }
        }

        public class HalConfigOptions
        {
            public string Name { get; set; }
            public bool AutoAcknowledge { get; set; }
        }
    }
}
