using System.Runtime.Serialization;

namespace Domain.Models.RabbitMQMessages
{
    [DataContract]
    public class SendConnectionsBody : PublishMessageBody
    {
        [DataMember(Name = "SendConnectionsStage", IsRequired = true)]
        public SendConnectionsStageBody SendConnectionsStage { get; set; }

        [DataMember(Name = "DailyLimit", IsRequired = true)]
        public int DailyLimit { get; set; }

        [DataMember(Name = "StartDateTimestamp", IsRequired = true)]
        public long StartDateTimestamp { get; set; }

        [DataMember(Name = "CampaignId", IsRequired = true)]
        public string CampaignId { get; set; }

    }
}
