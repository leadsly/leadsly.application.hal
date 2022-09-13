using System.Runtime.Serialization;

namespace Domain.Models.RabbitMQMessages
{
    [DataContract]
    public class MonitorForNewAcceptedConnectionsBody : PublishMessageBody
    {
        [DataMember]
        public string PageUrl { get; set; }
    }
}
