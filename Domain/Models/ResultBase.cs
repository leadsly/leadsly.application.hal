using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    [DataContract]
    public class ResultBase : IOperationResult
    {
        [DataMember]
        public bool Succeeded { get; set; }
        [DataMember]
        public List<Failure> Failures { get; set; }
    }
}
