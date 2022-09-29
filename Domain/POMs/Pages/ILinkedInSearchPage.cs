using OpenQA.Selenium;
using System.Collections.Generic;

namespace Domain.POMs.Pages
{
    public interface ILinkedInSearchPage
    {
        IList<IWebElement> GatherProspects(IWebDriver driver);
        void ScrollIntoView(IWebElement webElement, IWebDriver driver);
        bool IsNoSearchResultsContainerDisplayed(IWebDriver driver);
        bool AnyErrorPopUpMessages(IWebDriver webDriver);
        bool CloseErrorPopUpMessage(IWebDriver webDriver);
        bool? ClickRetrySearch(IWebDriver driver, int numberOfTries, int delayBetweenEachClick);
        bool SendConnectionRequest(IWebElement prospect, IWebDriver webDriver);
        bool ScrollFooterIntoView(IWebDriver webDriver);
        bool? IsPreviousButtonClickable(IWebDriver webDriver);
        IWebElement AreResultsHelpfulPTag(IWebDriver webDriver);
        bool WaitUntilSearchResultsFinishedLoading(IWebDriver webDriver);
        IWebElement GetProspectsActionButton(IWebElement prospect);
        bool IsSearchResultsPaginationDisplayed(IWebDriver webDriver);
        SearchResultsPageResult DetermineSearchResultsPage(IWebDriver webDriver);
    }
}
