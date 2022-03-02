using Domain.Pages;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Responses;
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
    public class LinkedInPage : ILinkedInPage
    {
        public LinkedInPage(IWebDriver driver, ILinkedInLoginPage linkedInLoginPage, ILinkedInHomePage linkedInHomePage, WebDriverWait wait, ILogger<LinkedInPage> logger)
        {
            this._driver = driver;
            this._logger = logger;
            this._wait = wait;
            this._linkedInLoginPage = linkedInLoginPage;            
            this._linkedInHomePage = linkedInHomePage;
        }
        private readonly ILogger<LinkedInPage> _logger;
        private readonly WebDriverWait _wait;
        private readonly IWebDriver _driver;
        private readonly ILinkedInLoginPage _linkedInLoginPage;
        private readonly ILinkedInHomePage _linkedInHomePage;

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
                    signInContainer = _driver.FindElement(By.ClassName("sign-in-form-container"));
                }
                catch(Exception ex)
                {

                }
                return signInContainer;
            }
        }

        public HalOperationResult<T> GoToPage<T>(string pageUrl)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            try
            {
                this._driver.Navigate().GoToUrl(new Uri(pageUrl));
            }
            catch(WebDriverTimeoutException timeoutEx)
            {
                throw timeoutEx;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, $"Failed to navigate to page {pageUrl}");
                return result;
            }

            result.Succeeded = true;
            return result;
        }
    }
}
