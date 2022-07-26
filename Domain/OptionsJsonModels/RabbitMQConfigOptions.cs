namespace Domain.OptionsJsonModels
{
    public class RabbitMQConfigOptions
    {
        public string RoutingKey { get; set; }
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
        public string VirtualHost { get; set; }
        public string ClientProvidedName { get; set; }
        public Ssl Ssl { get; set; }
    }

    public class Ssl
    {
        public bool Enabled { get; set; }
    }

    public class ExchangeConfigOptions
    {
        public string Name { get; set; }
        public string ExchangeType { get; set; }
    }

    public class QueueConfigOptions
    {
        public string Name { get; set; }
        public bool AutoAcknowledge { get; set; }
    }
}
