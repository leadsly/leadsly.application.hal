namespace Leadsly.Application.Model.RabbitMQ
{
    public class ConnectionFactoryOptions
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string HostName { get; set; }
        public int Port { get; set; }
        public ClientProvidedNameOptions ClientProvidedName { get; set; }
        public string VirtualHost { get; set; }

        public class ClientProvidedNameOptions
        {
            public string AppServer { get; set; }
            public string Hal { get; set; }
        }
    }
}
