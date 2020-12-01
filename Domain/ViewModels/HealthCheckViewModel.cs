using System.Runtime.Serialization;

namespace Domain.ViewModels
{
    [DataContract]
    public class HealthCheckViewModel
    {
        [DataMember(Name ="apiVersion")]
        public string APIVersion { get; set; }
    }
}
