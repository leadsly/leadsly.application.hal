using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Supervisor
{
    public partial class Supervisor : ISupervisor
    {
        public void Authenticate_Bot(string email, string password)
        {
            this._seleniumStartup.Authenticate(email, password);
        }
    }
}
