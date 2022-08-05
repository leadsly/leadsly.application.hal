using Leadsly.Application.Model.WebDriver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.Requests.Hal.Interfaces
{
    public interface IHalRequest
    {
        public string ServiceDiscoveryName { get; }
        public string NamespaceName { get; }
        public string RequestUrl { get; }
        public BrowserPurpose BrowserPurpose { get; set; }
        public long AttemptNumber { get; set; }
    }
}
