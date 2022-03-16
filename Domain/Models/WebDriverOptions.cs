using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class WebDriverOptions
    {
        public long DefaultImplicitWait { get; set; }
        public ChromeProfileConfigOptions ChromeProfileConfigOptions { get; set; }
    }

    public class ChromeProfileConfigOptions
    {
        public string Suffix { get; set; }
        public string DefaultChromeProfileName { get; set; }
        public string DefaultChromeUserProfilesDir { get; set; }
        public string ChromeProfileName { get; set; }
        public List<string> AddArguments { get; set; }
    }
}
