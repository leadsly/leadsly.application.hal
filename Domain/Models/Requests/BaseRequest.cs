using Leadsly.Application.Model.WebDriver;
using System.Runtime.Serialization;

namespace Domain.Models.Requests
{
    [DataContract]
    public class BaseRequest
    {
        [DataMember(IsRequired = false, EmitDefaultValue = true)]
        public string WindowHandleId { get; set; }

        [DataMember(IsRequired = true)]
        public BrowserPurpose BrowserPurpose { get; set; }

        [DataMember(IsRequired = true)]
        public long AttemptNumber { get; set; }

        [DataMember(IsRequired = true)]
        public string SidecartBaseUrl { get; set; }

        [DataMember(IsRequired = true)]
        public string SidecartRequestUrl { get; set; }
    }
}
