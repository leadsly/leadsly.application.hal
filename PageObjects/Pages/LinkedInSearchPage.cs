using Domain;
using Domain.POMs.Pages;
using Leadsly.Application.Model;
using Leadsly.Application.Model.LinkedInPages.SearchResultPage;
using Leadsly.Application.Model.LinkedInPages.SearchResultPage.Interfaces;
using Leadsly.Application.Model.Responses;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PageObjects.Pages
{
    public class LinkedInSearchPage : LeadslyBase, ILinkedInSearchPage
    {
        public LinkedInSearchPage(ILogger<LinkedInSearchPage> logger, IWebDriverUtilities webDriverUtilities) : base(logger)
        {
            _logger = logger;
            _webDriverUtilities = webDriverUtilities;
        }

        private readonly ILogger<LinkedInSearchPage> _logger;
        private readonly IWebDriverUtilities _webDriverUtilities;

        private IWebElement SearchResultFooter(IWebDriver webDriver)
        {
            IWebElement searchResultFooter = null;
            try
            {
                _logger.LogInformation("Retrieving search results footer and then trying to move it into view with 'MoveToElement' method");
                searchResultFooter = webDriver.FindElement(By.XPath("//ul[contains(@class, 'artdeco-pagination__pages--number')]/parent::div/parent::div"));
                Actions actions = new Actions(webDriver);
                actions.MoveToElement(searchResultFooter);
                actions.Perform();
            }
            catch (Exception ex)
            {
                _logger.LogDebug("Failed to locate search result navigational footer");
            }

            return searchResultFooter;
        }

        private IWebElement SearchResultsFooterUlElement(IWebElement searchResultsFooter)
        {
            IWebElement searchResultsFooterUl = default;
            try
            {
                searchResultsFooterUl = searchResultsFooter.FindElement(By.CssSelector(".artdeco-pagination .artdeco-pagination__pages"));
            }
            catch (Exception ex)
            {
                _logger.LogDebug("Failed to locate SearchResultsFooter ul element");
            }
            return searchResultsFooterUl;
        }

        private IWebElement LastPage(IWebDriver webDriver)
        {
            List<IWebElement> lis = default;
            try
            {
                _logger.LogTrace("Getting last page of the search results from the hitlist");
                IWebElement searchResultsFooter = _webDriverUtilities.WaitUntilNotNull(SearchResultFooter, webDriver, 10);
                if (searchResultsFooter == null)
                {
                    return null;
                }

                IWebElement ul = SearchResultsFooterUlElement(searchResultsFooter);
                if (ul == null)
                {
                    return null;
                }

                lis = ul.FindElements(By.TagName("li")).ToList();
                string last = lis.LastOrDefault()?.Text;
                _logger.LogTrace("Successfully found total hit list page result count. Last page is: {last}", last);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to locate last page in the navigational search result");
            }

            return lis?.LastOrDefault();
        }

        private Func<IWebDriver, IWebElement> SearchResultContainerQuery = (webDriver) => webDriver.FindElement(By.CssSelector("#main .search-results-container"));

        private IWebElement SearchResultsUlContainer(IWebDriver webDriver)
        {
            IWebElement searchResultUlContainer = default;
            try
            {
                searchResultUlContainer = webDriver.FindElement(By.ClassName("reusable-search__entity-result-list"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to locate search results container");
            }

            return searchResultUlContainer;
        }

        private List<IWebElement> ProspectsAsWebElements(IWebDriver webDriver)
        {
            List<IWebElement> prospects = default;
            try
            {
                prospects = SearchResultsUlContainer(webDriver).FindElements(By.TagName("li")).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ProspectsAsWebElements] Failed to locate prospects as li elements");
            }
            return prospects;
        }

        public HalOperationResult<T> GatherProspects<T>(IWebDriver driver) where T : IOperationResponse
        {
            HalOperationResult<T> result = new();
            IList<IWebElement> prospAsElements = _webDriverUtilities.WaitUntilNotNull(ProspectsAsWebElements, driver, 15);

            if (prospAsElements == null)
            {
                return result;
            }

            IGatherProspects prospects = new GatherProspects
            {
                ProspectElements = prospAsElements.ToList()
            };

            result.Value = (T)prospects;
            result.Succeeded = true;
            return result;
        }

        public IList<IWebElement>? GatherProspects(IWebDriver driver)
        {
            IList<IWebElement> prospects = _webDriverUtilities.WaitUntilNotNull(ProspectsAsWebElements, driver, 15);

            if (prospects == null)
            {
                return null;
            }

            return prospects;
        }

        public HalOperationResult<T> GetTotalSearchResults<T>(IWebDriver driver) where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            IWebElement numberOfPages = LastPage(driver);

            if (numberOfPages == null)
            {
                _logger.LogWarning("Could not determine the number of pages in the hitlist");
                result.Failures.Add(new()
                {
                    Code = Codes.WEBDRIVER_ERROR,
                    Reason = "Failed to locate last page element"
                });
                return result;
            }

            if (int.TryParse(numberOfPages.Text, out int resultCount) == false)
            {
                return result;
            }

            IGetTotalNumberOfResults numberOfResults = new GetTotalNumberOfResults
            {
                NumberOfResults = resultCount
            };

            result.Value = (T)numberOfResults;
            result.Succeeded = true;
            return result;
        }

        public int? GetTotalSearchResults(IWebDriver driver)
        {
            IWebElement numberOfPages = LastPage(driver);

            if (numberOfPages == null)
            {
                _logger.LogWarning("Could not determine the number of pages in the hitlist");
                return null;
            }

            if (int.TryParse(numberOfPages.Text, out int resultCount) == false)
            {
                return null;
            }

            return resultCount;
        }

        private IWebElement Footer(IWebDriver webDriver)
        {
            IWebElement footer = default;

            try
            {
                footer = webDriver.FindElement(By.CssSelector(".global-footer.global-footer--static"));
            }
            catch (Exception ex)
            {
                _logger.LogDebug("Failed to locate the footer");
            }

            return footer;
        }

        public HalOperationResult<T> ScrollFooterIntoView<T>(IWebDriver webDriver) where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            IWebElement footer = _webDriverUtilities.WaitUntilNotNull(Footer, webDriver, 10);
            if (footer == null)
            {
                _logger.LogTrace("Failed to locate the footer after waiting for 10 seconds");
                return result;
            }
            else
            {
                _logger.LogTrace("Executing javascript 'scrollIntoView' to scroll footer into view");
                IJavaScriptExecutor js = (IJavaScriptExecutor)webDriver;
                js.ExecuteScript("arguments[0].scrollIntoView({ behavior: 'smooth' });", footer);
            }

            result.Succeeded = true;
            return result;
        }

        private IWebElement NextButton(IWebDriver webDriver)
        {
            IWebElement nextBtn = default;
            try
            {
                nextBtn = webDriver.FindElement(By.CssSelector("button[aria-label='Next']"));
            }
            catch (Exception ex)
            {
                _logger.LogWarning("[NextButton] Failed to find next button");
            }

            return nextBtn;
        }

        private IWebElement PreviousButton(IWebDriver webDriver)
        {
            IWebElement previousBtn = default;
            try
            {
                previousBtn = webDriver.FindElement(By.CssSelector("button[aria-label='Previous']"));
            }
            catch (Exception ex)
            {
                _logger.LogWarning("[PreviousButton] Failed to find previous button");
            }

            return previousBtn;
        }

        public HalOperationResult<T> ClickNext<T>(IWebDriver driver) where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            IWebElement nextBtn = _webDriverUtilities.WaitUntilNotNull(NextButton, driver, 10);

            if (nextBtn == null)
            {
                _logger.LogWarning("[ClickNext]: Next button could not be located");
                return result;
            }

            try
            {
                _logger.LogTrace("Clicking next button");
                nextBtn.Click();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[ClickNext]: Failed to click next button");
                return result;
            }

            result.Succeeded = true;
            return result;
        }

        public bool? ClickNext(IWebDriver driver)
        {
            bool? succeeded = false;
            IWebElement nextBtn = _webDriverUtilities.WaitUntilNotNull(NextButton, driver, 10);
            if (nextBtn == null)
            {
                _logger.LogWarning("[ClickNext]: Next button could not be located");
                succeeded = null;
            }

            try
            {
                _logger.LogTrace("Clicking next button");
                nextBtn.Click();
                succeeded = true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[ClickNext]: Failed to click next button");
                succeeded = false;
            }

            return succeeded;
        }

        public HalOperationResult<T> ClickPrevious<T>(IWebDriver driver) where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            IWebElement previousBtn = _webDriverUtilities.WaitUntilNotNull(PreviousButton, driver, 10);

            if (previousBtn == null)
            {
                _logger.LogWarning("[ClickPrevious]: Previous button could not be located");
                return result;
            }

            try
            {
                _logger.LogTrace("Clicking previous button");
                previousBtn.Click();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[ClickPrevious]: Failed to click previous button");
                return result;
            }

            result.Succeeded = true;
            return result;
        }

        private IWebElement NoSearchResultsContainer(IWebDriver webDriver)
        {
            IWebElement noSearchResultsContainer = default;
            try
            {
                _logger.LogInformation("[NoSearchResultsContainer]: Searching for no search results container to see if we got an error page and need to retry the search");
                noSearchResultsContainer = webDriver.FindElement(By.CssSelector(".search-no-results__image-container"));
            }
            catch (Exception ex)
            {
                _logger.LogDebug("The NoSearchResultsContainer has not been found by class name '.search-no-results__image-container'");
            }

            return noSearchResultsContainer;
        }

        public bool IsNoSearchResultsContainerDisplayed(IWebDriver driver)
        {
            bool isDisplayed = false;
            try
            {
                IWebElement noResultsSearchContainer = _webDriverUtilities.WaitUntilNotNull(NoSearchResultsContainer, driver, 1);
                if (noResultsSearchContainer != null)
                {
                    isDisplayed = noResultsSearchContainer.Displayed;
                    _logger.LogDebug("NoSearchResultsContainer element was found. Is it displayed: {isDisplayed}", isDisplayed);
                }
            }
            catch (Exception ex)
            {
                // logging already occurs in the NoSearchResultsContainer method
            }

            return isDisplayed;
        }

        private IWebElement RetrySearchButton(IWebDriver webDriver)
        {
            IWebElement retrySearchButton = default;
            try
            {
                _logger.LogInformation("[RetrySearchButton]: Locating retry search button");
                retrySearchButton = webDriver.FindElement(By.CssSelector("button[data-test='no-results-cta']"));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[RetrySearchButton]: Failed to locate retry search button by 'button[data-test='no-results-cta']'");
            }
            return retrySearchButton;
        }

        public HalOperationResult<T> ClickRetrySearch<T>(IWebDriver driver) where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            IWebElement retryButton = RetrySearchButton(driver);
            if (retryButton == null)
            {
                return result;
            }

            try
            {
                _logger.LogTrace("[ClickRetrySearch] Clicking retry search results because we've gotten an error page");

                RandomWait(1, 5);

                retryButton.Click();

                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
                IWebElement searchResults = wait.Until<IWebElement>(webDriver =>
                {
                    _logger.LogTrace("[ClickRetrySearch] locating search results container");
                    IWebElement searchResultContainer = SearchResultContainerQuery(webDriver);
                    if (searchResultContainer == null)
                    {
                        _logger.LogTrace("[ClickRetrySearch] Failed to locate search results container. Attemping to click retry button again");
                        retryButton.Click();
                    }
                    return searchResultContainer;
                });

                if (searchResults == null)
                {
                    _logger.LogTrace("[ClickRetrySearch] Search results container is null");
                    return result;
                }

            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to click on retry search button");
                return result;
            }

            result.Succeeded = true;
            return result;
        }

        public bool? ClickRetrySearch(IWebDriver driver, int numberOfTries, int delayBetweenEachClick)
        {
            bool? succeeded = false;
            IWebElement retryButton = RetrySearchButton(driver);
            if (retryButton == null)
            {
                _logger.LogWarning("Failed to locate 'Retry Search' button");
                succeeded = null;
                return succeeded;
            }

            try
            {
                _logger.LogTrace("[ClickRetrySearch] Clicking retry search results because we've gotten an error page");
                RandomWait(delayBetweenEachClick, delayBetweenEachClick);
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
                int counter = 0;
                IWebElement searchResults = wait.Until<IWebElement>(webDriver =>
                {
                    _logger.LogTrace("[ClickRetrySearch] locating search results container");
                    IWebElement searchResultContainer = SearchResultContainerQuery(webDriver);
                    if (searchResultContainer == null)
                    {
                        if (counter <= numberOfTries)
                        {
                            _logger.LogTrace("[ClickRetrySearch] Failed to locate search results container. Attemping to click retry button again");
                            retryButton.Click();
                            counter++;
                        }
                    }
                    return searchResultContainer;
                });
                succeeded = true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to click on retry search button");
                succeeded = false;
            }

            return succeeded;
        }

        public HalOperationResult<T> SendConnectionRequest<T>(IWebElement prospect) where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            IWebElement actionButton = GetProspectsActionBtn(prospect);
            if (actionButton == null)
            {
                _logger.LogDebug("[SendConnectionRequest] Action button not found");
                // prospect must not contain an action button
                return result;
            }

            _logger.LogDebug("[SendConnectionRequest] Action button was found.");
            if (actionButton.Text == ApiConstants.PageObjectConstants.Connect)
            {
                _logger.LogDebug("[SendConnectionRequest] Action button text is 'Connect'");
                result = ClickButton<T>(actionButton);
                if (result.Succeeded == false)
                {
                    _logger.LogDebug("Failed to click 'Connect' button for the prospect");
                    return result;
                }
            }
            else
            {
                string actionButtonText = actionButton.Text;
                _logger.LogDebug("[SendConnectionRequest] Action button text is '{actionButtonText}'", actionButtonText);
                return result;
            }

            result.Succeeded = true;
            return result;
        }

        private HalOperationResult<T> ClickButton<T>(IWebElement btn)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new();
            try
            {
                btn.Click();
            }
            catch (Exception ex)
            {
                // let the caller log the error
            }

            result.Succeeded = true;
            return result;
        }

        private IWebElement GetProspectsActionBtn(IWebElement prospect)
        {
            IWebElement actionButton = default;
            try
            {
                _logger.LogInformation("Finding prospect's action button");
                actionButton = prospect.FindElement(By.CssSelector(".entity-result__actions div:not([class='entity-result__actions--empty']"));
                actionButton = actionButton.FindElement(By.TagName("button"));
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Prospect does not contain action button");
            }
            return actionButton;
        }

        public IWebElement GetProspectsActionButton(IWebElement prospect)
        {
            return GetProspectsActionBtn(prospect);
        }

        public IWebElement GetSendInvitationModal(IWebDriver webDriver)
        {
            _logger.LogInformation("Finding 'Send Invite' modal");
            IWebElement modal = _webDriverUtilities.WaitUntilNotNull(GetSendInviteModal, webDriver, 5);
            return modal;
        }

        private IWebElement GetSendInviteModal(IWebDriver webDriver)
        {
            IWebElement modal = default;
            try
            {
                _logger.LogInformation("Locating send invite modal by css selector '#artdeco-modal-outlet .artdeco-modal'");
                modal = webDriver.FindElement(By.CssSelector("#artdeco-modal-outlet .artdeco-modal"));
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Failed to locate 'Send Invite' modal");
            }
            return modal;
        }

        public bool IsNextButtonDisabled(IWebDriver webDriver)
        {
            IWebElement nextButton = NextButton(webDriver);

            if (nextButton == null)
            {
                return false;
            }

            string disabledAttribute = nextButton.GetAttribute("disabled");
            bool.TryParse(disabledAttribute, out bool result);
            return result;
        }

        public bool? IsNextButtonClickable(IWebDriver webDriver)
        {
            IWebElement nextButton = _webDriverUtilities.WaitUntilNotNull(NextButton, webDriver, 5);

            if (nextButton == null)
            {
                return null;
            }

            string disabledAttribute = nextButton.GetAttribute("disabled");
            bool.TryParse(disabledAttribute, out bool result);
            // if the button does not contain disabled attribute, the return value will be null. out bool will produce false. So we want to flip the condition
            return !result;
        }

        private IWebElement ResultsHeaderH2(IWebDriver webDriver)
        {
            IWebElement resultsHeader = default;
            try
            {
                resultsHeader = webDriver.FindElement(By.CssSelector(".search-results-container h2"));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Unable to results header");
            }
            return resultsHeader;
        }

        public IWebElement ResultsHeader(IWebDriver webDriver)
        {
            IWebElement resultsHeader = _webDriverUtilities.WaitUntilNotNull(ResultsHeaderH2, webDriver, 15);

            return resultsHeader;
        }

        private IWebElement AreTheseResultsHelpfulPTag(IWebDriver webDriver)
        {
            IWebElement areResultsHelpfulPTag = default;
            try
            {
                areResultsHelpfulPTag = webDriver.FindElement(By.XPath("//div[contains(@class, 'search-explicit-feedback-in-content-detail')]/descendant::p[contains(., 'Are these results helpful?')]"));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to locate are these results helpful P tag");
            }
            return areResultsHelpfulPTag;
        }

        public IWebElement AreResultsHelpfulPTag(IWebDriver webDriver)
        {
            IWebElement resultsHelpfulPTag = _webDriverUtilities.WaitUntilNotNull(AreTheseResultsHelpfulPTag, webDriver, 2);

            return resultsHelpfulPTag;
        }

        private IWebElement SearchResultLoader(IWebDriver webDriver)
        {
            IWebElement searchResultsLoader = default;
            try
            {
                searchResultsLoader = webDriver.FindElement(By.CssSelector(".core-rail .search-result-loader__container"));
            }
            catch (Exception)
            {
                _logger.LogInformation("Search results container was not found");
            }
            return searchResultsLoader;
        }

        public HalOperationResult<T> WaitUntilSearchResultsFinishedLoading<T>(IWebDriver webDriver)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            IWebElement searchResultLoader = _webDriverUtilities.WaitUntilNull(SearchResultLoader, webDriver, 60);

            if (searchResultLoader != null)
            {
                return result;
            }

            result.Succeeded = true;
            return result;
        }

        public bool WaitUntilSearchResultsFinishedLoading(IWebDriver webDriver)
        {
            bool succeeded = false;
            IWebElement searchResultLoader = _webDriverUtilities.WaitUntilNull(SearchResultLoader, webDriver, 60);

            if (searchResultLoader != null)
            {
                _logger.LogDebug("Search results loader was still present");
                succeeded = false;
            }

            succeeded = true;
            return succeeded;
        }

        private IWebElement LinkedInLogoFooter(IWebDriver webDriver)
        {
            IWebElement logo = default;
            try
            {
                logo = webDriver.FindElement(By.CssSelector("footer li-icon[aria-label='LinkedIn']"));
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to locate LinkedIn logo in the footer");
            }
            return logo;
        }

        public IWebElement LinkInFooterLogoIcon(IWebDriver webDriver)
        {
            IWebElement logo = _webDriverUtilities.WaitUntilNotNull(LinkedInLogoFooter, webDriver, 10);

            return logo;
        }

        public void ScrollIntoView(IWebElement webElement, IWebDriver webDriver)
        {
            try
            {
                if (webElement != null)
                {
                    _logger.LogTrace("Executing javascript 'scrollIntoView' to scroll element into view");
                    IJavaScriptExecutor js = (IJavaScriptExecutor)webDriver;
                    js.ExecuteScript("arguments[0].scrollIntoView({ behavior: 'smooth' });", webElement);
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug("Could not execute scroll into view method on the given element");
            }
        }

        private IWebElement SearchLimitDiv(IWebDriver driver)
        {
            IWebElement limitDiv = default;
            try
            {
                limitDiv = driver.FindElement(By.ClassName("search-nec__simple-image"));
            }
            catch (Exception ex)
            {
                _logger.LogDebug("Tried looking for monthly search limit div, but couldn't find it using '.search-nec__simple-image'");
            }
            return limitDiv;
        }

        public bool MonthlySearchLimitReached(IWebDriver driver)
        {
            IWebElement searchLimitDiv = SearchLimitDiv(driver);
            return searchLimitDiv != null;
        }

        public void ScrollTop(IWebDriver webDriver)
        {
            _webDriverUtilities.ScrollTop(webDriver);
        }

        public SearchResultsPageResult DetermineSearchResultsPage(IWebDriver webDriver)
        {
            _logger.LogInformation("Determining SignIn status. This is used to determine if user is already authenticated or not");
            SearchResultsPageResult result = SearchResultsPageResult.None;
            try
            {
                WebDriverWait wait = new WebDriverWait(webDriver, TimeSpan.FromSeconds(30));
                wait.Until(drv =>
                {
                    result = SearchResultsContainerResult(drv);
                    return result != SearchResultsPageResult.Unknown;
                });
            }
            catch (Exception ex)
            {
                _logger.LogDebug("WebDrivers wait method timedout. This means that the maximum allowed wait time elapsed and the element was not found. Wait time in seconds: ", 30);
            }
            return result;
        }

        private SearchResultsPageResult SearchResultsContainerResult(IWebDriver webDriver)
        {
            IWebElement searchResultsContainer = _webDriverUtilities.WaitUntilNotNull(SearchResultsDiv, webDriver, 3);
            if (searchResultsContainer != null)
            {
                return SearchResultsPageResult.Results;
            }

            IWebElement noSearchResultsContainer = _webDriverUtilities.WaitUntilNotNull(NoSearchResultsDiv, webDriver, 3);
            if (noSearchResultsContainer != null)
            {
                return SearchResultsPageResult.NoResults;
            }

            return SearchResultsPageResult.Unknown;
        }

        private IWebElement NoSearchResultsDiv(IWebDriver webDriver)
        {
            IWebElement noSearchResultsDiv = default;
            try
            {
                noSearchResultsDiv = webDriver.FindElement(By.ClassName("reusable-search-filters__no-results"));
            }
            catch (Exception ex)
            {
                _logger.LogDebug("Tried looking for no search results div, but couldn't find it using '.reusable-search-filters__no-results'");
            }
            return noSearchResultsDiv;
        }

        private IWebElement SearchResultsDiv(IWebDriver webDriver)
        {
            IWebElement searchResultsDiv = default;
            try
            {
                searchResultsDiv = webDriver.FindElement(By.ClassName("search-results-container"));
            }
            catch (Exception ex)
            {
                _logger.LogDebug("Tried looking for no search results div, but couldn't find it using '.search-results-container'");
            }
            return searchResultsDiv;
        }
    }
}
