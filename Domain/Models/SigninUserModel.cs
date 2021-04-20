using System.Runtime.Serialization;

namespace Domain.Models
{
    [DataContract]
    public class SigninUserModel
    {
        [DataMember(Name = "email", EmitDefaultValue = false)]
        public string Email { get; set; }

        [DataMember(Name = "password", EmitDefaultValue = false)]
        public string Password  { get; set; }
    }
}
