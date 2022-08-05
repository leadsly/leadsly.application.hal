using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.Responses.Hal.Interfaces
{
    public interface IOperationInformation
    {
        public bool TabClosed { get; set; }
        public bool BrowserClosed { get; set; }
        public string WindowHandleId { get; set; }
        public bool WebDriverError { get; set; }
        public string HalId { get; set; }
        public bool ShouldOperationBeRetried { get; set; }
        public List<Failure> Failures { get; set; }
    }
}
