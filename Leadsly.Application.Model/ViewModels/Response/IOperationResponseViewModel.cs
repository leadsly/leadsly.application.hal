using Leadsly.Application.Model.ViewModels.Response.Hal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.ViewModels.Response
{
    public interface IOperationResponseViewModel
    {
        public List<FailureViewModel> Failures { get; set; }
    }
}
