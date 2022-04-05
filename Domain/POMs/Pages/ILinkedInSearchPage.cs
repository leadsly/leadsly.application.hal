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
    public interface ILinkedInSearchPage
    {
        HalOperationResult<T> GetTotalSearchResults<T>(IWebDriver driver)
            where T : IOperationResponse;

        HalOperationResult<T> GatherProspects<T>(IWebDriver driver)
            where T : IOperationResponse;

        HalOperationResult<T> ClickNext<T>(IWebDriver driver)
            where T : IOperationResponse;

        bool IsNoSearchResultsContainerDisplayed(IWebDriver driver);

        HalOperationResult<T> ClickRetrySearch<T>(IWebDriver driver)
            where T : IOperationResponse;

        HalOperationResult<T> SendConnectionRequest<T>(IWebElement prospect)
            where T : IOperationResponse;

        HalOperationResult<T> ClickSendInModal<T>(IWebDriver webDriver)
            where T : IOperationResponse;

        bool IsNextButtonDisabled(IWebDriver webDriver);
        
    }
}
