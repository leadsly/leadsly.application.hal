using System.Runtime.Serialization;

namespace Domain.ViewModels
{
    [DataContract]
    public class SigninUserModel
    {
        [DataMember(Name = "email", EmitDefaultValue = false)]
        public string Email { get; set; }

        [DataMember(Name = "password", EmitDefaultValue = false)]
        public string Password  { get; set; }

        [DataMember(Name = "rememberMe", EmitDefaultValue = true)]
        public bool RememberMe { get; set; }
    }
}
