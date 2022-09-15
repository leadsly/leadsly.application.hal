using System.Runtime.Serialization;

namespace Domain.MQ.Messages
{
    [DataContract]
    public class MonitorForNewAcceptedConnectionsBody : PublishMessageBody
    {
        [DataMember]
        public string PageUrl { get; set; }
    }
}
