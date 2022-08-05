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
    public class EnterTwoFactorAuthCodeResponse : IEnterTwoFactorAuthCodeResponse
    {
        [DataMember]
        public bool InvalidOrExpiredCode { get; set; }
        [DataMember]
        public bool UnexpectedErrorOccured { get; set; }
        [DataMember]
        public OperationInformation OperationInformation { get; set; }
    }
}
