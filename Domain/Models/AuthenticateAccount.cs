using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class AuthenticateAccount
    {
        public string WebDriverId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string ConnectAuthUrl { get; set; }
    }
}
