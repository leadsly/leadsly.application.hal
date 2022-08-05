using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model
{
    public interface IOperationResult
    {
        public bool Succeeded { get; set; }        
    }
}
