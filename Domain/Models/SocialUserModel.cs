using Domain.Models;
using System.Runtime.Serialization;

namespace Domain.Models
{
    [DataContract]
    public class SocialUserModel
    {
        [DataMember(Name = "id", EmitDefaultValue = false)]
        public string Id { get; set; }

        [DataMember(Name = "email", EmitDefaultValue = false)]
        public string Email { get; set; }

        [DataMember(Name = "firstName", EmitDefaultValue = false)]
        public string FirstName { get; set; }

        [DataMember(Name = "lastName", EmitDefaultValue = false)]
        public string LastName { get; set; }

        [DataMember(Name = "photoUrl", EmitDefaultValue = false)]
        public string PhotoUrl { get; set; }

        [DataMember(Name = "authToken", EmitDefaultValue = false)]
        public string AuthToken { get; set; }

        [DataMember(Name = "idToken", EmitDefaultValue = false)]
        public string IdToken { get; set; }

        [DataMember(Name = "provider", EmitDefaultValue = false)]
        public string Provider { get; set; }
    }
}
