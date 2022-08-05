using Leadsly.Application.Model.WebDriver;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model
{
    public class WebDriverOperationData
    {
        public BrowserPurpose BrowserPurpose { get; set; }
        public string RequestedWindowHandleId { get; set; }
        public string ChromeProfileName { get; set; }
        public string PageUrl { get; set; }
        public List<string> PageUrls { get; set; }
    }
}
