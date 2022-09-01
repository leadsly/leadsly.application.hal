using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Domain.Models.Requests
{
    [DataContract]
    public class ProspectsRepliedRequest
    {
        [DataMember]
        public string HalId { get; set; }
        [DataMember]
        public string NamespaceName { get; set; }
        [DataMember]
        public string ServiceDiscoveryName { get; set; }
        [DataMember]
        public string RequestUrl { get; set; }
        [DataMember]
        public IList<ProspectReplied> Items { get; set; }
    }
}
