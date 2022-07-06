using System.Runtime.Serialization;

namespace Domain.Models.Requests
{
    [DataContract]
    public class TwoFactorAuthRequest
    {
        [DataMember(IsRequired = true)]
        public string Code { get; set; }
    }
}
