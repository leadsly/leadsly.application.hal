using Leadsly.Application.Model;
using Leadsly.Application.Model.Responses;
using OpenQA.Selenium;
using System.Collections.Generic;

namespace Domain.POMs.Pages
{
    public interface ILinkedInSearchPage
    {
        HalOperationResult<T> GetTotalSearchResults<T>(IWebDriver driver)
            where T : IOperationResponse;

        int? GetTotalSearchResults(IWebDriver driver);

        bool MonthlySearchLimitReached(IWebDriver driver);

        HalOperationResult<T> GatherProspects<T>(IWebDriver driver)
            where T : IOperationResponse;

        IList<IWebElement> GatherProspects(IWebDriver driver);

        void ScrollIntoView(IWebElement webElement, IWebDriver driver);

        HalOperationResult<T> ClickNext<T>(IWebDriver driver)
            where T : IOperationResponse;

        bool? ClickNext(IWebDriver driver);

        HalOperationResult<T> ClickPrevious<T>(IWebDriver driver)
            where T : IOperationResponse;

        bool IsNoSearchResultsContainerDisplayed(IWebDriver driver);

        HalOperationResult<T> ClickRetrySearch<T>(IWebDriver driver)
            where T : IOperationResponse;

        bool? ClickRetrySearch(IWebDriver driver, int numberOfTries, int delayBetweenEachClick);

        HalOperationResult<T> SendConnectionRequest<T>(IWebElement prospect)
            where T : IOperationResponse;

        HalOperationResult<T> ScrollFooterIntoView<T>(IWebDriver webDriver)
            where T : IOperationResponse;

        IWebElement GetSendInvitationModal(IWebDriver webDriver);

        bool IsNextButtonDisabled(IWebDriver webDriver);
        bool? IsNextButtonClickable(IWebDriver webDriver);

        IWebElement ResultsHeader(IWebDriver webDriver);

        IWebElement AreResultsHelpfulPTag(IWebDriver webDriver);

        IWebElement LinkInFooterLogoIcon(IWebDriver webDriver);

        HalOperationResult<T> WaitUntilSearchResultsFinishedLoading<T>(IWebDriver webDriver)
            where T : IOperationResponse;

        bool WaitUntilSearchResultsFinishedLoading(IWebDriver webDriver);

        IWebElement GetProspectsActionButton(IWebElement prospect);

        void ScrollTop(IWebDriver webDriver);

        SearchResultsPageResult DetermineSearchResultsPage(IWebDriver webDriver);

    }
}
