using Leadsly.Application.Model.Responses.Hal.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.Responses.Hal
{
    [DataContract]
    public class OperationInformation : IOperationInformation
    {
        [DataMember]
        public bool TabClosed { get; set; }
        [DataMember]
        public string WindowHandleId { get; set; } = string.Empty;
        [DataMember]
        public bool BrowserClosed { get; set; }
        [DataMember]
        public bool WebDriverError { get; set; }
        [DataMember]
        public string HalId { get; set; }
        [DataMember]
        public bool ShouldOperationBeRetried { get; set; }
        [DataMember]
        // in case there is a re-try that should happen lets preserve the failures
        public List<Failure> Failures { get; set; } = new();
    }
}
