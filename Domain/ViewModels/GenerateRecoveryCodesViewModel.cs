using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Domain.ViewModels
{
    [DataContract]
    public class GenerateRecoveryCodesViewModel
    {
        [DataMember(Name = "items", EmitDefaultValue = false)]
        public IList<string> Items { get; set; }
    }
}
