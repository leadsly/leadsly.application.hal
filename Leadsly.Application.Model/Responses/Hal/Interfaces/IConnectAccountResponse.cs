using Leadsly.Application.Model.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.Responses.Hal.Interfaces
{
    public interface IConnectAccountResponse : IOperationResponse
    {        
        public bool TwoFactorAuthRequired { get; }
        public TwoFactorAuthType TwoFactorAuthType { get; }
        public bool UnexpectedErrorOccured { get; }
    }
}
