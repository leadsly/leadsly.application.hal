using Leadsly.Application.Model;
using Leadsly.Application.Model.Responses;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.POMs.Pages
{
    public interface ILinkedInPage
    {
        public bool IsAuthenticationRequired(IWebDriver webDriver);
        public bool IsSignInContainerDisplayed(IWebDriver webDriver);
        HalOperationResult<T> GoToPage<T>(IWebDriver webDriver, string pageUrl)
            where T : IOperationResponse;
    }
}
