using System.Runtime.Serialization;

namespace Domain.Models.Requests
{
    [DataContract]
    public class SentFollowUpMessageRequest
    {
        [DataMember]
        public string ServiceDiscoveryName { get; set; } = string.Empty;
        [DataMember]
        public string NamespaceName { get; set; } = string.Empty;
        [DataMember]
        public string RequestUrl { get; set; } = string.Empty;
        [DataMember]
        public string CampaignProspectId { get; set; }
        [DataMember]
        public int MessageOrderNum { get; set; }
        [DataMember]
        public long ActualDeliveryDateTimeStamp { get; set; }
    }
}
