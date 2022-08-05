using Leadsly.Application.Model;
using Leadsly.Application.Model.Requests;
using Leadsly.Application.Model.Requests.Hal;
using Leadsly.Application.Model.Requests.Hal.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.Requests.Hal
{
    public class NewWebDriverRequest : BaseHalRequest, INewWebDriverRequest
    {
        public int DefaultTimeoutInSeconds { get; set; }
    }
}
