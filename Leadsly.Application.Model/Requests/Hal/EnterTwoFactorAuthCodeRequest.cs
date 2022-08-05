using Leadsly.Application.Model.Requests.Hal;
using Leadsly.Application.Model.Requests.Hal.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.Requests.Hal
{
    public class EnterTwoFactorAuthCodeRequest : BaseHalRequest, IEnterTwoFactorAuthCodeRequest
    {
        public string Code { get; set; }
        public string WindowHandleId { get; set; }
    }
}
