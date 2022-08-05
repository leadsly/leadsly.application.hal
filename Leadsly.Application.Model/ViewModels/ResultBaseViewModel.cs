using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.ViewModels
{
    public class ResultBaseViewModel : IOperationResultViewModel, IOperationResultDetailViewModel
    {
        public bool Succeeded { get; set; } = false;
        public List<FailureViewModel> Failures { get; set; } = new();
        public ProblemDetails ProblemDetails { get ; set; }
        public bool WebDriverError { get; set; }
        public string WindowHandleId { get; set; }
        public bool TabClosed { get; set; }
        public string HalId { get; set; }
        public bool BrowserClosed { get; set; }        
        public bool ShouldOperationBeRetried { get; set; }
    }
}
