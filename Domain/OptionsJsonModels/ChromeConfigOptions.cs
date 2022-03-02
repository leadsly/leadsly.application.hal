using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.OptionsJsonModels
{
    public class ChromeConfigOptions
    {
        public string DefaultProfile { get; set; }
        public string ChromeUserDirectory { get; set; }
        public List<string> AddArguments { get; set; }
        public int WebDriverWaitFromSeconds { get; set; }

    }
}
