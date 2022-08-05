using Leadsly.Application.Model;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Leadsly.Application.Model.ViewModels
{
    [DataContract]
    public class AuthenticatorSetupResultViewModel
    {
        [DataMember(Name = "status", EmitDefaultValue = true)]
        public TwoFactorAuthenticationStatus Status {get; set;}

        [DataMember(Name = "recoveryCodes", EmitDefaultValue = false)]
        public UserRecoveryCodesViewModel RecoveryCodes { get; set; }
    }
}
