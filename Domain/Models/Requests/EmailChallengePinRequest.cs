using System.Runtime.Serialization;

namespace Domain.Models.Requests
{
    [DataContract]
    public class EmailChallengePinRequest
    {
        [DataMember(IsRequired = true)]
        public string Pin { get; set; }
    }
}
