namespace Leadsly.Application.Model.RabbitMQ
{
    public class QueueOptions
    {
        public AppServerOptions AppServer { get; set; }
        public HalOptions Hal { get; set; }

        public class AppServerOptions
        {
            public string Name { get; set; }
            public bool AutoAcknowledge { get; set; }
        }

        public class HalOptions
        {
            public string Name { get; set; }
            public bool AutoAcknowledge { get; set; }
        }
    }
}
