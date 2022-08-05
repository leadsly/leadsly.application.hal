using Leadsly.Application.Model.Responses.Hal;
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
    public class NewWebDriverResponse : ResultBase, INewWebDriverResponse
    {
        [DataMember]
        public string WebDriverId { get; set; }
        [DataMember]
        public OperationInformation OperationInformation { get; set; }
    }
}
