namespace Leadsly.Application.Model.RabbitMQ
{
    public class RabbitMQOptions
    {
        public RoutingKeyOptions RoutingKey { get; set; }
        public ConnectionFactoryOptions ConnectionFactoryOptions { get; set; }
        public ExchangeOptions ExchangeOptions { get; set; }
        public QueueOptions QueueConfigOptions { get; set; }

        public class RoutingKeyOptions
        {
            public string AppServer { get; set; }
            public string Hal { get; set; }
        }
    }
}
