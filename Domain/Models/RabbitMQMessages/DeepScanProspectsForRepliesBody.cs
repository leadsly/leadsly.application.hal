using System.Runtime.Serialization;

namespace Domain.Models.RabbitMQMessages
{
    [DataContract]
    public class DeepScanProspectsForRepliesBody : PublishMessageBody
    {
        [DataMember]
        public string PageUrl { get; set; } = string.Empty;
    }
}
