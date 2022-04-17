using Domain.POMs.Pages;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Responses;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using System;

namespace PageObjects.Pages
{
    public class LinkedInPage : LeadslyBase, ILinkedInPage
    {
        public LinkedInPage(ILogger<LinkedInPage> logger) : base(logger)
        {
            this._logger = logger;
        }

        private readonly ILogger<LinkedInPage> _logger;

        public bool IsAuthenticationRequired(IWebDriver webDriver)
        {            
            return SignInContainer(webDriver) != null;            
        }

        private IWebElement SignInContainer(IWebDriver webDriver)
        {
            IWebElement signInContainer = null;
            try
            {
                signInContainer = webDriver.FindElement(By.ClassName("sign-in-form-container"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to locate SignInContainer by class name 'sign-in-form-container'");
            }
            return signInContainer;
        }

        public HalOperationResult<T> GoToPage<T>(IWebDriver webDriver, string pageUrl) where T : IOperationResponse
        {
            return base.GoToPageUrl<T>(webDriver, pageUrl);
        }
    }
}
