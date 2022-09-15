using System.Runtime.Serialization;

namespace Domain.Models.Requests
{
    [DataContract]
    public class LinkedInSignInRequest
    {
        [DataMember(IsRequired = true)]
        public string Username { get; set; }

        [DataMember(IsRequired = true)]
        public string Password { get; set; }

        [DataMember(IsRequired = true)]
        public string GridServiceDiscoveryName { get; set; }

        [DataMember(IsRequired = true)]
        public string GridNamespaceName { get; set; }

        [DataMember(IsRequired = true)]
        public string ProxyServiceDiscoveryName { get; set; }

        [DataMember(IsRequired = true)]
        public string ProxyNamespaceName { get; set; }
    }
}
