using System.Runtime.Serialization;

namespace Domain.Models.Requests
{
    [DataContract]
    public class LinkedInSignInRequest
    {
        [DataMember(IsRequired = true)]
        public string Username { get; set; }

        [DataMember(IsRequired = true)]
        public string Password { get; set; }
    }
}
