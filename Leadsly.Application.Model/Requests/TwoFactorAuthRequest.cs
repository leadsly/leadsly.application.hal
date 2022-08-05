using Leadsly.Application.Model;
using Leadsly.Application.Model.Requests;
using Leadsly.Application.Model.WebDriver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.Requests
{
    [DataContract]
    public class TwoFactorAuthRequest : AccountBaseRequest, IInfoForHal
    {
        [DataMember(Name = "code")]
        public string Code { get; set; }

        [DataMember(Name = "windowHandleId")]
        public string WindowHandleId { get; set; }

        [DataMember(Name = "browserPurpose")]
        public BrowserPurpose BrowserPurpose { get; set; }
        [DataMember(Name = "attemptNumber")]
        public long AttemptNumber { get; set; }

    }
}
