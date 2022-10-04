using Domain;
using Domain.POMs;
using Domain.POMs.Pages;
using Domain.Services.Interfaces;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Responses;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PageObjects.Pages
{
    public class LinkedInSearchPage : ILinkedInSearchPage
    {
        public LinkedInSearchPage(ILogger<LinkedInSearchPage> logger, IWebDriverUtilities webDriverUtilities, IHumanBehaviorService humanBehaviorService)
        {
            _logger = logger;
            _webDriverUtilities = webDriverUtilities;
            _humanBehaviorService = humanBehaviorService;
        }

        private readonly IHumanBehaviorService _humanBehaviorService;
        private readonly ILogger<LinkedInSearchPage> _logger;
        private readonly IWebDriverUtilities _webDriverUtilities;
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

        public IList<IWebElement>? GatherProspects(IWebDriver driver)
        {
            IList<IWebElement> prospects = _webDriverUtilities.WaitUntilNotNull(ProspectsAsWebElements, driver, 15);

            if (prospects == null)
            {
                return null;
            }

            return prospects;
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

        private IWebElement PaginatorDiv(IWebDriver webDriver)
        {
            IWebElement paginationDiv = default;
            try
            {
                paginationDiv = webDriver.FindElement(By.CssSelector(".artdeco-pagination"));
            }
            catch (Exception ex)
            {
                _logger.LogDebug("Failed to locate the pagination div");
            }

            return paginationDiv;
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

        public bool ScrollFooterIntoView(IWebDriver webDriver)
        {
            bool succeeded = false;
            IWebElement footer = _webDriverUtilities.WaitUntilNotNull(Footer, webDriver, 10);
            if (footer == null)
            {
                _logger.LogTrace("Failed to locate the footer after waiting for 10 seconds");
                return false;
            }
            else
            {
                _logger.LogTrace("Executing javascript 'scrollIntoView' to scroll footer into view");
                IJavaScriptExecutor js = (IJavaScriptExecutor)webDriver;
                js.ExecuteScript("arguments[0].scrollIntoView({ behavior: 'smooth' });", footer);
            }

            return true;
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
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
                int counter = 0;
                IWebElement searchResults = wait.Until<IWebElement>(webDriver =>
                {
                    _humanBehaviorService.RandomWaitSeconds(delayBetweenEachClick, delayBetweenEachClick);
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

        public bool SendConnectionRequest(IWebElement prospect, IWebDriver webDriver)
        {
            IWebElement actionButton = GetProspectsActionBtn(prospect);
            if (actionButton == null)
            {
                _logger.LogDebug("[SendConnectionRequest] Action button not found");
                // prospect must not contain an action button
                return false;
            }

            webDriver.ScrollTop(_humanBehaviorService);

            if (webDriver.IsElementVisible(actionButton) == false)
            {
                _webDriverUtilities.ScrollIntoView(actionButton, webDriver);

                _humanBehaviorService.RandomWaitMilliSeconds(950, 1670);
            }

            _logger.LogDebug("[SendConnectionRequest] Action button was found.");
            if (actionButton.Text == ApiConstants.PageObjectConstants.Connect)
            {
                _logger.LogDebug("[SendConnectionRequest] Action button text is {0}", ApiConstants.PageObjectConstants.Connect);
                if (_webDriverUtilities.HandleClickElement(actionButton) == false)
                {
                    _logger.LogDebug("Failed to click 'Connect' button for the prospect");
                    return false;
                }
            }
            else
            {
                string actionButtonText = actionButton.Text;
                _logger.LogDebug("[SendConnectionRequest] Action button text is '{actionButtonText}'", actionButtonText);
                return false; ;
            }

            return true;
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

        public bool IsSearchResultsPaginationDisplayed(IWebDriver webDriver)
        {
            if (ScrollFooterIntoView(webDriver) == false)
            {
                return false;
            }

            IWebElement paginator = _webDriverUtilities.WaitUntilNotNull(PaginatorDiv, webDriver, 5);
            // if paginator is null return false else return true
            if (paginator == null)
            {
                return false;
            }

            return paginator.Displayed;
        }

        public bool? IsPreviousButtonClickable(IWebDriver webDriver)
        {
            IWebElement previousButton = _webDriverUtilities.WaitUntilNotNull(PreviousButton, webDriver, 5);

            if (previousButton == null)
            {
                return null;
            }

            string disabledAttribute = previousButton.GetAttribute("disabled");
            bool.TryParse(disabledAttribute, out bool result);
            // if the button does not contain disabled attribute, the return value will be null. out bool will produce false. So we want to flip the condition
            return !result;
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
                _logger.LogWarning("Failed to locate are these results helpful P tag");
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

        public bool AnyErrorPopUpMessages(IWebDriver webDriver)
        {
            IWebElement toastError = _webDriverUtilities.WaitUntilNotNull(ErrorToast, webDriver, 2);
            return toastError != null;
        }

        public bool CloseErrorPopUpMessage(IWebDriver webDriver)
        {
            IWebElement toastError = _webDriverUtilities.WaitUntilNotNull(ErrorToast, webDriver, 2);
            if (toastError != null)
            {
                IWebElement closePopUpButton = ClosePopUpButton(toastError);
                return _webDriverUtilities.HandleClickElement(closePopUpButton);
            }

            return false;
        }

        private IWebElement ClosePopUpButton(IWebElement popup)
        {
            IWebElement closePopUpButton = default;
            try
            {
                closePopUpButton = popup.FindElement(By.XPath("//div[contains(@class, 'artdeco-toast-item')] //descendant:: li-icon[@type='cancel-icon']/ancestor::button"));
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Could not locate pop ups close button");
            }
            return closePopUpButton;
        }

        private IWebElement ErrorToast(IWebDriver webDriver)
        {
            IWebElement toastMessage = default;
            try
            {
                toastMessage = webDriver.FindElement(By.CssSelector(".artdeco-toasts_toasts div[data-test-artdeco-toast-item-type='error']"));
            }
            catch (Exception ex)
            {
                _logger.LogDebug("No error toast messages have been found");
            }

            return toastMessage;
        }
    }
}
