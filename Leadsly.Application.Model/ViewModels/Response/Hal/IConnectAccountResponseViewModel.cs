using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.ViewModels.Response.Hal
{
    public interface IConnectAccountResponseViewModel : IOperationResponseViewModel
    {
        public bool TwoFactorAuthRequired { get; }
        public TwoFactorAuthType TwoFactorAuthType { get; }
        public bool UnexpectedErrorOccured { get; }
    }
}
