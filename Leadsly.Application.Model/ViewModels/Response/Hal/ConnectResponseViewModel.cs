using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.ViewModels.Response.Hal
{
    public class ConnectResponseViewModel : IConnectAccountResponseViewModel
    {
        public bool TwoFactorAuthRequired { get; set; }

        public TwoFactorAuthType TwoFactorAuthType { get; set; }

        public bool UnexpectedErrorOccured { get; set; }
        public List<FailureViewModel> Failures { get; set; } = new();
    }
}
