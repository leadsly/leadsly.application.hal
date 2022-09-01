using System.Runtime.Serialization;

namespace Domain.Models.RabbitMQMessages.AppServer
{
    [DataContract]
    public class TriggerScanProspectsForRepliesMessage
    {
        [DataMember]
        public string UserId { get; set; }
        [DataMember]
        public string HalId { get; set; }
    }
}
