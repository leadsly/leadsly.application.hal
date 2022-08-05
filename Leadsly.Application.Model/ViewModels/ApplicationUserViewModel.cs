using System.Runtime.Serialization;

namespace Leadsly.Application.Model.ViewModels
{
    [DataContract]
    public class ApplicationUserViewModel
    {
        [DataMember(Name = "id", EmitDefaultValue = true)]
        public string Id { get; set; }

        [DataMember(Name = "email", EmitDefaultValue = true)]
        public string Email { get; set; }

        [DataMember(Name = "userName", EmitDefaultValue = false)]
        public string UserName { get; set; }

        [DataMember(Name = "firstName", EmitDefaultValue = false)]
        public string FirstName { get; set; }

        [DataMember(Name = "lastName", EmitDefaultValue = false)]        
        public string LastName { get; set; }
    }
}
