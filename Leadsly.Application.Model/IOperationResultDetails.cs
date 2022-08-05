using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model
{
    public interface IOperationResultDetails
    {
        public List<Failure> Failures { get; set; }
        public ProblemDetails ProblemDetails { get; set; }
    }
}
