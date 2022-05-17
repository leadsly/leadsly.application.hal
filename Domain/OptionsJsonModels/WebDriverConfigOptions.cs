﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.OptionsJsonModels
{
    public class WebDriverConfigOptions
    {
        public long DefaultImplicitWait { get; set; }
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
        public string DefaultProfile { get; set; }
        public string ChromeUserDirectory { get; set; }
        public List<string> AddArguments { get; set; }
    }
}
