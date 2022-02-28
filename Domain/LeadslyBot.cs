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
    public class LeadslyBot : ILeadslyBot
    {
        public ChromeOptions DriverOptions
        {
            get;
            set;
        }

        public LeadslyBot(ILogger<LeadslyBot> logger)
        {
            _logger = logger;
        }

        private readonly ILogger<LeadslyBot> _logger;

        public LinkedInLoginPage Authenticate(IWebDriver driver, string email, string password)
        {
            
            LinkedInLoginPage loginPage = new LinkedInLoginPage(driver, this._logger);

            loginPage.EnterEmail(email);

            loginPage.EnterPassword(password);

            loginPage.SignIn();

            return loginPage;
        }

        public LinkedInPage GoToLinkedIn(IWebDriver driver)
        {
            LinkedInPage linkedInPage = new LinkedInPage(driver, this._logger);

            if (driver.Url.Contains("LinkedIn.com") == false)
            {
                linkedInPage.GoToPage();
            }            

            return linkedInPage;
        }
    }
}
