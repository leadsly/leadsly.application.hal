using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PageObjects.Pages
{
    public class LinkedInHomePage
    {
        public LinkedInHomePage(IWebDriver driver, ILogger logger)
        {
            this._driver = driver;
            this._logger = logger;
            this._wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));                   
        }
        private readonly ILogger _logger;
        private readonly WebDriverWait _wait;
        private readonly IWebDriver _driver;



    }
}
