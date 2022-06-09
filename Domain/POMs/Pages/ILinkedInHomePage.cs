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
    public interface ILinkedInHomePage
    {
        public HalOperationResult<T> GoToPage<T>(IWebDriver webDriver, string pageUrl)
            where T : IOperationResponse;

        bool IsNewsFeedDisplayed(IWebDriver webDriver);

        void WaitUntilNewsFeedIsDisplayed(IWebDriver webDriver);

    }
}
