using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Domain.ViewModels
{
    [DataContract]
    public class AuthenticatorSetupResultViewModel
    {
        [DataMember(Name = "status", EmitDefaultValue = true)]
        public TwoFactorAuthenticationStatus Status {get; set;}

        [DataMember(Name = "recoveryCodes", EmitDefaultValue = false)]
        public List<string> RecoveryCodes { get; set; }
    }
}
