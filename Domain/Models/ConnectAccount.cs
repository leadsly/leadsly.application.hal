using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class ConnectAccount : WebDriverDetails
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public ConnectedAccountType AccountType { get; set; }        
    }
}
