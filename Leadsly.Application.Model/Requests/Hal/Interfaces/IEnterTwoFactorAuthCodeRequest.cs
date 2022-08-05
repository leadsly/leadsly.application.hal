using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.Requests.Hal.Interfaces
{
    public interface IEnterTwoFactorAuthCodeRequest : IHalRequest
    {
        public string Code { get; }
        public string WindowHandleId { get; }
    }
}
