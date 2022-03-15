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
    public class LinkedInPage : LeadslyWebDriverBase, ILinkedInPage
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
                catch(Exception ex)
                {

                }
                return signInContainer;
            
        }
    }
}
