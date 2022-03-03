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
    public class AuthenticateAccountRequest : BaseRequest
    {        
        [DataMember(IsRequired = true)]
        public string Username { get; set; }

        [DataMember(IsRequired = true)]
        public string Password { get; set; }

        [DataMember(IsRequired = false)]
        public string ConnectAuthUrl { get; set; }
    }
}
