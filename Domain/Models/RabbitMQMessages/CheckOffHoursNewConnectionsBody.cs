using System.Runtime.Serialization;

namespace Domain.Models.RabbitMQMessages
{
    [DataContract]
    public class CheckOffHoursNewConnectionsBody : PublishMessageBody
    {
        [DataMember]
        public string TimezoneId { get; set; }

        [DataMember(IsRequired = false)]
        public int NumOfHoursAgo { get; set; }
    }
}
