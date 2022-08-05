using Leadsly.Application.Model.WebDriver;
using System.Runtime.Serialization;

namespace Leadsly.Application.Model.Requests
{
    [DataContract]
    public class ConnectAccountRequest : AccountBaseRequest
    {
        [DataMember(Name = "password", IsRequired = true)]
        public string Password { get; set; }

        [DataMember(Name = "browserPurpose", IsRequired = true)]
        public BrowserPurpose BrowserPurpose { get; set; }

        [DataMember(Name = "attemptNumber", IsRequired = false)]
        public long AttemptNumber { get; set; }
    }
}
