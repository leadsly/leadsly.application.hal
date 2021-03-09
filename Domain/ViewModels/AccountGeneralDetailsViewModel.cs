using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Domain.ViewModels
{
    [DataContract]
    public class AccountGeneralDetailsViewModel
    {
        [DataMember(Name = "email", IsRequired = true, EmitDefaultValue = true)]
        public string Email { get; set; }
        [DataMember(Name = "verified", IsRequired = true, EmitDefaultValue = true)]
        public bool Verified { get; set; }
    }
}
