using Domain.Models.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    [DataContract]
    public class TwoFactorAuthenticationRequest : BaseRequest
    {
        [DataMember(IsRequired = true)]
        public string Code { get; set; }        
    }
}
