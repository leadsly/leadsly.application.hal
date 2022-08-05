
using System.Runtime.Serialization;

namespace Leadsly.Application.Model.ViewModels
{
    [DataContract]
    public class ChangeEmailRequestViewModel
    {
        [DataMember(Name = "newEmail", EmitDefaultValue = false)]
        public string NewEmail { get; set; }

        [DataMember(Name = "password", EmitDefaultValue = false)]
        public string Password { get; set; }
    }
}
