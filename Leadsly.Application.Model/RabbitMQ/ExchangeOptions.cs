namespace Leadsly.Application.Model.RabbitMQ
{
    public class ExchangeOptions
    {
        public AppServerOptions AppServer { get; set; }
        public HalOptions Hal { get; set; }

        public class AppServerOptions
        {
            public string Name { get; set; }
            public string ExchangeType { get; set; }
        }

        public class HalOptions
        {
            public string Name { get; set; }
            public string ExchangeType { get; set; }
        }
    }
}
