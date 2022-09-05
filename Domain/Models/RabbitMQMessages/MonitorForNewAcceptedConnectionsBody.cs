using System.Runtime.Serialization;

namespace Domain.Models.RabbitMQMessages
{
    [DataContract]
    public class MonitorForNewAcceptedConnectionsBody : PublishMessageBody
    {
        [DataMember]
        public string PageUrl { get; set; }

        [DataMember]
        public string TimeZoneId { get; set; }

        [DataMember(IsRequired = false)]
        public int NumOfHoursAgo { get; set; }
    }
}
