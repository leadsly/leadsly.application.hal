using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.Requests.Hal.Interfaces
{
    public interface IConnectAccountRequest : IHalRequest
    {
        public string Username { get; }
        public string Password { get; }
        public string ConnectAuthUrl { get; }
    }
}
