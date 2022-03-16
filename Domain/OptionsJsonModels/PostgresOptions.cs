using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.OptionsJsonModels
{
    public class PostgresOptions
    {
        public string Database { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string UserId { get; set; }
    }
}
