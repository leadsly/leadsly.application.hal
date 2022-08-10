using System.Runtime.Serialization;

namespace Leadsly.Application.Model.Campaigns
{
    [DataContract]
    public class PublishMessageBody
    {
        [DataMember]
        public string ServiceDiscoveryName { get; set; } = string.Empty;

        [DataMember]
        public string NamespaceName { get; set; } = string.Empty;

        [DataMember]
        public string RequestUrl { get; set; } = string.Empty;

        [DataMember]
        public string HalId { get; set; } = string.Empty;

        [DataMember]
        public string UserId { get; set; } = string.Empty;

        [DataMember(IsRequired = true)]
        public string TimeZoneId { get; set; } = string.Empty;

        [DataMember(IsRequired = true)]
        public string EndOfWorkday { get; set; } = string.Empty;

        [DataMember(IsRequired = true)]
        public string StartOfWorkday { get; set; } = string.Empty;

        [DataMember]
        public string ChromeProfileName { get; set; } = string.Empty;

    }
}
