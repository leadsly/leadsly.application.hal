using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Leadsly.Application.Model.ViewModels
{
    [DataContract]
    public class PasswordChangeViewModel
    {
        [DataMember(Name = "currentPassword", EmitDefaultValue = false)]
        public string CurrentPassword { get; set; }

        [DataMember(Name = "password", EmitDefaultValue = false)]
        public string NewPassword { get; set; }

        [DataMember(Name = "confirmPassword", EmitDefaultValue = false)]
        public string ConfirmPassword { get; set; }
    }
}
