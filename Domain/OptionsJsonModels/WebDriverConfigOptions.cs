using System.Collections.Generic;

namespace Domain.OptionsJsonModels
{
    public class WebDriverConfigOptions
    {
        public bool? UseGrid { get; set; }
        public string ProfilesVolume { get; set; }
        public long DefaultImplicitWait { get; set; }
        public long PageLoadTimeout { get; set; }
        public ChromeConfigOptions ChromeConfigOptions { get; set; }
        public SeleniumGridConfigOptions SeleniumGridConfigOptions { get; set; }
    }

    public class SeleniumGridConfigOptions
    {
        public string Url { get; set; }
        public long Port { get; set; }
    }

    public class ChromeConfigOptions
    {
        public Proxy Proxy { get; set; }
        public string DefaultProfile { get; set; }
        public string ChromeUserDirectory { get; set; }
        public List<string> AddArguments { get; set; }
    }

    public class Proxy
    {
        public string HttpProxy { get; set; }
    }
}
