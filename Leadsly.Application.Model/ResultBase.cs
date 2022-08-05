using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model
{
    [DataContract]
    public class ResultBase : IOperationResult, IOperationResultDetails
    {
        [DataMember(IsRequired = true)]
        public bool Succeeded { get; set; } = false;     

        [DataMember]
        public List<Failure> Failures { get; set; } = new();

        [DataMember]
        public ProblemDetails ProblemDetails { get; set; }

    }
}
