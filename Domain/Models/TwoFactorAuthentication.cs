using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class TwoFactorAuthentication
    {
        public string WebDriverId { get; set; }
        public string Code { get; set; }        
    }
}
