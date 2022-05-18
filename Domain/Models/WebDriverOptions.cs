using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class WebDriverOptions
    {
        public bool? UseGrid { get; set; }
        public string ProfilesVolume { get; set; }
        public long DefaultImplicitWait { get; set; }
        public ChromeProfileConfigOptions ChromeProfileConfigOptions { get; set; }
        public SeleniumGrid SeleniumGrid { get; set; }

    }

    public class SeleniumGrid
    {
        public string Url { get; set; }
        public long Port { get; set; }
    }

    public class ChromeProfileConfigOptions
    {
        public string DefaultChromeProfileName { get; set; }
        public string DefaultChromeUserProfilesDir { get; set; }
        public string ChromeProfileName { get; set; }
        public List<string> AddArguments { get; set; }
    }
}
