using System.Runtime.Serialization;

namespace Domain.Models
{
    [DataContract]
    public class TwoFactorAuthenticationVerificationCodeModel
    {
        [DataMember(Name = "code", EmitDefaultValue = false)]
        public string Code { get; set; }
        [DataMember(Name = "provider", EmitDefaultValue = false)]
        public string Provider { get; set; }
        [DataMember(Name = "email", EmitDefaultValue = false)]
        public string Email { get; set; }
    }
}
