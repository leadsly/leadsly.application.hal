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
        public LinkedInPage(ILogger<LinkedInPage> logger, IWebDriverUtilities webDriverUtilities) : base(logger)
        {
            this._logger = logger;
            _webDriverUtilities = webDriverUtilities;
        }

        private readonly ILogger<LinkedInPage> _logger;
        private readonly IWebDriverUtilities _webDriverUtilities;

        public bool IsAuthenticationRequired(IWebDriver webDriver)
        {            
            return SignInContainer(webDriver) != null;            
        }

        public bool IsSignInContainerDisplayed(IWebDriver webDriver)
        {
            _logger.LogDebug("Checking if the sign in container is displayed, waiting for 3 seconds");
            IWebElement signInContainer = _webDriverUtilities.WaitUntilNotNull(SignInContainer, webDriver, 3);

            return signInContainer != null;
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
