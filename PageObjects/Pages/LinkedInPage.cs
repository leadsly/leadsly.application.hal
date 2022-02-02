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
    public class LinkedInPage
    {
        private const string LinkedInUrl = "https://linkedin.com";
        public LinkedInPage(IWebDriver driver, ILogger logger)
        {
            this._driver = driver;
            this._logger = logger;
            this._wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            this.LinkedInLoginPage = new LinkedInLoginPage(driver, logger);
            this.LinkedInHomePage = new LinkedInHomePage(driver, logger);
        }
        private readonly ILogger _logger;
        private readonly WebDriverWait _wait;
        private readonly IWebDriver _driver;

        public LinkedInLoginPage LinkedInLoginPage { get; private set; }
        public LinkedInHomePage LinkedInHomePage { get; set; }

        public bool IsAuthenticationRequired
        {
            get
            {
                return SignInContainer != null;
            }
        }

        private IWebElement SignInContainer
        {
            get
            {
                IWebElement signInContainer = null;
                try
                {
                    signInContainer = _wait.Until(drv => drv.FindElement(By.ClassName("sign-in-form-container")));
                }
                catch(Exception ex)
                {

                }
                return signInContainer;
            }
        }

        public void GoToPage()
        {
            try
            {
                this._driver.Manage().Window.Maximize();
            }
            catch(WebDriverTimeoutException timeoutEx)
            {
                throw timeoutEx;
            }
            catch(Exception ex)
            {
                _logger.LogWarning("[LinkedInPage] Failed to maximize window.");
            }

            this._driver.Navigate().GoToUrl(new Uri(LinkedInUrl));
        }
    }
}
