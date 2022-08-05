using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.ViewModels.Response.Hal
{
    public class EnterTwoFactorAuthCodeResponseViewModel : IEnterTwoFactorAuthCodeResponseViewModel
    {
        public bool InvalidOrExpiredCode { get; set; }
        public bool UnexpectedErrorOccured { get; set; }
        public List<FailureViewModel> Failures { get; set; } = new();
    }
}
