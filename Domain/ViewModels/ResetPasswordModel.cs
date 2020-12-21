using System.Runtime.Serialization;

namespace Domain.ViewModels
{
    [DataContract]
    public class ResetPasswordModel
    {
        [DataMember(Name = "email", EmitDefaultValue = false)]
        public string Email { get; set; }

        [DataMember(Name = "password", EmitDefaultValue = false)]
        public string Password { get; set; }

        [DataMember(Name = "passwordResetToken", EmitDefaultValue = false)]
        public string PasswordResetToken { get; set; }
    }
}
