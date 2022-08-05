using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.ViewModels
{
    [DataContract]
    public class AccountInviteViewModel
    {
        [DataMember(Name = "error", IsRequired = true, EmitDefaultValue = true)]
        public bool Error { get; set; }

        [DataMember(Name = "errorDescription", IsRequired = true, EmitDefaultValue = false)]
        public string ErrorDescription { get; set; }
    }
}
