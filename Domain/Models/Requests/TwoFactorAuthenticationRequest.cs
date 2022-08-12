using System.Runtime.Serialization;

namespace Domain.Models.Requests
{
    [DataContract]
    public class TwoFactorAuthenticationRequest : BaseRequest
    {
        [DataMember(IsRequired = true)]
        public string Code { get; set; }

        [DataMember(IsRequired = true)]
        public string GridNamespaceName { get; set; }

        [DataMember(IsRequired = true)]
        public string GridServiceDiscoveryName { get; set; }
    }
}
