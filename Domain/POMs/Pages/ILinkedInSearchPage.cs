using Leadsly.Application.Model;
using Leadsly.Application.Model.Responses;
using OpenQA.Selenium;

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

        HalOperationResult<T> ClickPrevious<T>(IWebDriver driver)
            where T : IOperationResponse;

        bool IsNoSearchResultsContainerDisplayed(IWebDriver driver);

        HalOperationResult<T> ClickRetrySearch<T>(IWebDriver driver)
            where T : IOperationResponse;

        HalOperationResult<T> SendConnectionRequest<T>(IWebElement prospect)
            where T : IOperationResponse;

        HalOperationResult<T> ScrollFooterIntoView<T>(IWebDriver webDriver)
            where T : IOperationResponse;

        IWebElement GetSendInvitationModal(IWebDriver webDriver);

        bool IsNextButtonDisabled(IWebDriver webDriver);

        IWebElement ResultsHeader(IWebDriver webDriver);

        IWebElement AreResultsHelpfulPTag(IWebDriver webDriver);

        IWebElement LinkInFooterLogoIcon(IWebDriver webDriver);

        HalOperationResult<T> WaitUntilSearchResultsFinishedLoading<T>(IWebDriver webDriver)
            where T : IOperationResponse;

        IWebElement GetProspectsActionButton(IWebElement prospect);

        void ScrollTop(IWebDriver webDriver);
    }
}
