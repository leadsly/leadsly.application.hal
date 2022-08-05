using Leadsly.Application.Model.Requests.Hal.Interfaces;
using Leadsly.Application.Model.WebDriver;
using System.Runtime.Serialization;

namespace Leadsly.Application.Model.Requests
{
    [DataContract]
    public class BaseHalRequest : IHalRequest
    {
        [DataMember(Name = "ServiceDiscoveryName", IsRequired = false)]
        public string ServiceDiscoveryName { get; set; } = string.Empty;
        [DataMember(Name = "NamespaceName", IsRequired = false)]
        public string NamespaceName { get; set; } = string.Empty;
        [DataMember(Name = "RequestUrl", IsRequired = false)]
        public string RequestUrl { get; set; } = string.Empty;
        [DataMember(Name = "HalId", IsRequired = true)]
        public string HalId { get; set; } = string.Empty;
        [DataMember(Name = "BrowserPurpose", IsRequired = false)]
        public BrowserPurpose BrowserPurpose { get; set; }
        [DataMember(Name = "AttemptNumber", IsRequired = false)]
        public long AttemptNumber { get; set; }        
    }
}
