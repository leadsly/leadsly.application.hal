using Leadsly.Application.Model.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.Requests.Hal
{
    public class HealthCheckRequest : BaseHalRequest
    {
        public string PrivateIpAddress { get; set; }
    }
}
