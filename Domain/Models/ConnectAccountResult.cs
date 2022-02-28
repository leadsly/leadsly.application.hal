using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class ConnectAccountResult : WebDriverDetails
    {
        public bool Succeeded { get; set; } = false;
        public bool TwoFactorAuthRequired { get; set; } = false;
        public bool UnexpectedErrorOccured { get; set; } = false;        
        public TwoFactorAuthType TwoFactorAuthType { get; set; }
        public List<Failure> Failures { get; set; } = new();
    }
}
