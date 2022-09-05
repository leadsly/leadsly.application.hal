using Leadsly.Application.Model;
using Leadsly.Application.Model.Responses;
using OpenQA.Selenium;
using System.Collections.Generic;

namespace Domain.POMs.Pages
{
    public interface ILinkedInSearchPage
    {

        IList<IWebElement> GatherProspects(IWebDriver driver);

        void ScrollIntoView(IWebElement webElement, IWebDriver driver);

        bool IsNoSearchResultsContainerDisplayed(IWebDriver driver);

        bool? ClickRetrySearch(IWebDriver driver, int numberOfTries, int delayBetweenEachClick);

        HalOperationResult<T> SendConnectionRequest<T>(IWebElement prospect)
            where T : IOperationResponse;

        bool ScrollFooterIntoView(IWebDriver webDriver);
        bool? IsPreviousButtonClickable(IWebDriver webDriver);

        IWebElement AreResultsHelpfulPTag(IWebDriver webDriver);

        bool WaitUntilSearchResultsFinishedLoading(IWebDriver webDriver);

        IWebElement GetProspectsActionButton(IWebElement prospect);

        bool IsSearchResultsPaginationDisplayed(IWebDriver webDriver);

        SearchResultsPageResult DetermineSearchResultsPage(IWebDriver webDriver);

    }
}
