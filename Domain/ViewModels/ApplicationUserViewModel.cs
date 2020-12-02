using System.Runtime.Serialization;

namespace Domain.ViewModels
{
    [DataContract]
    public class ApplicationUserViewModel
    {
        [DataMember(Name = "id", EmitDefaultValue = true)]
        public string Id { get; set; }

        [DataMember(Name = "firstName", EmitDefaultValue = false)]
        public string FirstName { get; set; }

        [DataMember(Name = "lastName", EmitDefaultValue = false)]        
        public string LastName { get; set; }
    }
}
