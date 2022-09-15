using System.Collections.Generic;

namespace Domain.Models
{
    public class WebDriverOptions
    {
        public bool? UseGrid { get; set; }
        public string ProfilesVolume { get; set; }
        public long DefaultImplicitWait { get; set; }
        public long PagLoadTimeout { get; set; }
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
        public Proxy Proxy { get; set; }
        public string DefaultChromeProfileName { get; set; }
        public string DefaultChromeUserProfilesDir { get; set; }
        public string ChromeProfileName { get; set; }
        public List<string> AddArguments { get; set; }


    }

    public class Proxy
    {
        public string HttpProxy { get; set; }
    }
}
