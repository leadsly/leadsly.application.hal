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

        bool MonthlySearchLimitReached(IWebDriver driver);

        HalOperationResult<T> GatherProspects<T>(IWebDriver driver)
            where T : IOperationResponse;

        void ScrollIntoView(IWebElement webElement, IWebDriver driver);

        HalOperationResult<T> ClickNext<T>(IWebDriver driver)
            where T : IOperationResponse;

        bool IsNoSearchResultsContainerDisplayed(IWebDriver driver);

        HalOperationResult<T> ClickRetrySearch<T>(IWebDriver driver)
            where T : IOperationResponse;

        HalOperationResult<T> SendConnectionRequest<T>(IWebElement prospect)
            where T : IOperationResponse;

        HalOperationResult<T> ClickSendInModal<T>(IWebDriver webDriver)
            where T : IOperationResponse;

        HalOperationResult<T> ScrollFooterIntoView<T>(IWebDriver webDriver)
            where T : IOperationResponse;

        IWebElement GetCustomizeThisInvitationModalElement(IWebDriver webDriver);

        IWebElement GetCustomizeThisInvitationModalContent(IWebDriver webDriver);

        bool IsNextButtonDisabled(IWebDriver webDriver);

        IWebElement ResultsHeader(IWebDriver webDriver);

        HalOperationResult<T> WaitForResultsHeader<T>(IWebDriver webDriver)
            where T : IOperationResponse;

        IWebElement AreResultsHelpfulPTag(IWebDriver webDriver);

        IWebElement LinkInFooterLogoIcon(IWebDriver webDriver);

        HalOperationResult<T> WaitUntilSearchResultsFinishedLoading<T>(IWebDriver webDriver)
            where T : IOperationResponse;
    }
}
