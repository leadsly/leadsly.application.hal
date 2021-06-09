using System.Runtime.Serialization;

namespace Domain.ViewModels
{
    [DataContract]
    public class EmailChangeViewModel
    {
        [DataMember(Name = "newEmail", EmitDefaultValue = false)]
        public string NewEmail { get; set; }

        [DataMember(Name = "password", EmitDefaultValue = false)]
        public string Password { get; set; }

        [DataMember(Name = "emailChangeToken", EmitDefaultValue = false)]
        public string EmailChangeToken { get; set; }
    }
}
