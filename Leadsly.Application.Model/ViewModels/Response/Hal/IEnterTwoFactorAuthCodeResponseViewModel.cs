using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.ViewModels.Response.Hal
{
    public interface IEnterTwoFactorAuthCodeResponseViewModel : IOperationResponseViewModel
    {
        public bool InvalidOrExpiredCode { get; set; }
        public bool UnexpectedErrorOccured { get; set; }
    }
}
