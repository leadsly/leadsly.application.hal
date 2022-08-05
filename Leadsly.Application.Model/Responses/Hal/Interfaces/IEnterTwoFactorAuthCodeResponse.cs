using Leadsly.Application.Model.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.Responses.Hal.Interfaces
{
    public interface IEnterTwoFactorAuthCodeResponse : IOperationResponse
    {
        public bool InvalidOrExpiredCode { get; }
        public bool UnexpectedErrorOccured { get; }
    }
}
