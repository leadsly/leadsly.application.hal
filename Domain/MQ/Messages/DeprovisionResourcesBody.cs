using System.Runtime.Serialization;

namespace Domain.MQ.Messages
{
    [DataContract]
    public class DeprovisionResourcesBody
    {
        [DataMember]
        public string UserId { get; set; }

        [DataMember]
        public string HalId { get; set; }
    }
}
