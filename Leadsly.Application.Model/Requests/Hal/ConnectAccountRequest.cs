using Leadsly.Application.Model.Requests.Hal.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.Requests.Hal
{
    public class ConnectAccountRequest : BaseHalRequest, IConnectAccountRequest
    {
        public string WindowHandleId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string ConnectAuthUrl { get; set; }
        public long AttemptNumber { get; set; }
    }
}
