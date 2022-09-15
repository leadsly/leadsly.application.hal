using System.Runtime.Serialization;

namespace Domain.MQ.Messages
{
    [DataContract]
    public class TriggerSendFollowUpMessages
    {
        [DataMember]
        public string UserId { get; set; }
        [DataMember]
        public string HalId { get; set; }
    }
}
