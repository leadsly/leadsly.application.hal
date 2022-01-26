using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using PageObjects.Pages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public class SeleniumStartup : ISeleniumStartup
    {
        public ChromeOptions DriverOptions
        {
            get;
            set;
        }

        public SeleniumStartup(ILogger<SeleniumStartup> logger)
        {
            _logger = logger;
            ChromeOptions options = new ChromeOptions
            {
                
            };

            //options.AddArgument("headless");
            _driver = new ChromeDriver(options);
            _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
            DriverOptions = options;
        }

        private readonly ChromeDriver _driver;
        private readonly ILogger<SeleniumStartup> _logger;

        public void Authenticate(string email, string password)
        {
            LinkedInLoginPage loginPage = new LinkedInLoginPage(this._driver, this._logger);

            loginPage.GoToPage();

            loginPage.EnterEmail(email);

            loginPage.EnterPassword(password);

            loginPage.SignIn();
        }
    }
}
