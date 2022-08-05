using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Leadsly.Application.Model.ViewModels
{
    [DataContract]
    public class AccountSecurityDetailsViewModel
    {
        [DataMember(Name = "hasAuthenticator", EmitDefaultValue = true)]
        public bool HasAuthenticator { get; set; }
        [DataMember(Name = "twoFactorEnabled", EmitDefaultValue = true)]
        public bool TwoFactorEnabled { get; set; }
        [DataMember(Name = "recoveryCodesLeft", EmitDefaultValue = true)]
        public int RecoveryCodesLeft { get; set; }
        [DataMember(Name = "recoveryCodes", EmitDefaultValue = true)]
        public UserRecoveryCodesViewModel RecoveryCodes { get; set; }
        [DataMember(Name = "externalLogins", EmitDefaultValue = true)]
        public IList<string> ExternalLogins { get; set; }
    }
}
