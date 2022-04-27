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
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PageObjects.Pages
{
    public class LinkedInSearchPage : LeadslyBase, ILinkedInSearchPage
    {
        public LinkedInSearchPage(ILogger<LinkedInSearchPage> logger) : base(logger)
        {
            _logger = logger;
        }

        private readonly ILogger<LinkedInSearchPage> _logger;
        private const int Timeout = 30;

        private IWebElement SearchResultFooter(IWebDriver webDriver)
        {
            IWebElement searchResultFooter = null;
            try
            {
                _logger.LogInformation("Retrieving search results footer and then trying to move it into view with 'MoveToElement' method");
                searchResultFooter = webDriver.FindElement(By.CssSelector(".artdeco-card.mb6"));
                Actions actions = new Actions(webDriver);
                actions.MoveToElement(searchResultFooter);
                actions.Perform();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to locate search result navigational footer");
            }

            return searchResultFooter;
        }

        private IWebElement LastPage(IWebDriver webDriver)
        {
            List<IWebElement> lis = default;
            try
            {
                _logger.LogTrace("Getting last page of the search results from the hitlist");
                IWebElement ul = SearchResultFooter(webDriver).FindElement(By.CssSelector(".artdeco-pagination .artdeco-pagination__pages"));
                lis = ul.FindElements(By.TagName("li")).ToList();
                string last = lis.LastOrDefault()?.Text;
                _logger.LogTrace("Successfully found total hit list page result count. Last page is: {last}", last);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to locate last page in the navigational search result");
            }

            return lis.LastOrDefault();
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

        //private IWebElement HitList(IWebDriver webDriver)
        //{
        //    IWebElement hitList = default;
        //    try
        //    {
        //        _logger.LogInformation("Temporarily changing web driver's implicit wait time to {Timeout}", Timeout);
        //        TimeSpan defaultTimeSpan = webDriver.Manage().Timeouts().ImplicitWait;
        //        webDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(Timeout);

        //        hitList = SearchResultsContainer(webDriver).FindElement(By.CssSelector("div"));

        //        double defaultTimeout = defaultTimeSpan.TotalSeconds;
        //        _logger.LogInformation("Reverting web drivers timeout back to it's default value {defaultTimeout}", defaultTimeout);
        //        webDriver.Manage().Timeouts().ImplicitWait = defaultTimeSpan;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Failed to locate search hitlist");
        //    }

        //    return hitList;
        //}

        //private IWebElement ProspectList(IWebDriver webDriver)
        //{
        //    IWebElement propsectListUlNode = default;
        //    try
        //    {

        //        propsectListUlNode = HitList(webDriver).FindElement(By.TagName("ul"));
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "[ProspectList] Failed to locate the propsect list 'ul' node");
        //    }

        //    return propsectListUlNode;
        //}

        private List<IWebElement> ProspectsAsWebElements(IWebDriver webDriver)
        {
            List<IWebElement> prospects = default;
            try
            {
                _logger.LogTrace("[ProspectsAsWebElements] Temporarily changing web driver's implicit wait time to {Timeout}", Timeout);
                TimeSpan defaultTimeSpan = webDriver.Manage().Timeouts().ImplicitWait;
                webDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(Timeout);

                prospects = SearchResultsUlContainer(webDriver).FindElements(By.TagName("li")).ToList();

                double defaultTimeout = defaultTimeSpan.TotalSeconds;
                _logger.LogTrace("[ProspectsAsWebElements] Reverting web drivers timeout back to it's default value {defaultTimeout}", defaultTimeout);
                webDriver.Manage().Timeouts().ImplicitWait = defaultTimeSpan;
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

            List<IWebElement> prospAsElements = ProspectsAsWebElements(driver);
            if(prospAsElements == null)
            {
                return result;
            }

            IGatherProspects prospects = new GatherProspects
            {
                ProspectElements = prospAsElements
            };

            result.Value = (T)prospects;
            result.Succeeded = true;
            return result;
        }

        public HalOperationResult<T> GetTotalSearchResults<T>(IWebDriver driver) where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            IWebElement numberOfPages = LastPage(driver);

            if(numberOfPages == null)
            {
                _logger.LogWarning("Could not determine the number of pages in the hitlist");
                result.Failures.Add(new()
                {
                    Code = Codes.WEBDRIVER_ERROR,
                    Reason = "Failed to locate last page element"                    
                });
                return result;
            }

            if(int.TryParse(numberOfPages.Text, out int resultCount) == false)
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

        private IWebElement NextButton(IWebDriver webDriver)
        {
            IWebElement nextBtn = default;
            try
            {
                _logger.LogTrace("[NextButton] Temporarily changing web driver's implicit wait time to {Timeout}", Timeout);
                TimeSpan defaultTimeSpan = webDriver.Manage().Timeouts().ImplicitWait;
                webDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(Timeout);

                IWebElement footer = webDriver.FindElement(By.CssSelector(".global-footer.global-footer--static"));

                if (footer == null)
                {
                    _logger.LogTrace("[NextButton] Footer could not be located. Returning null");
                    return null;
                }
                else
                {
                    _logger.LogTrace("[NextButton] Executing javascript 'scrollIntoView' to scroll footer into view");
                    IJavaScriptExecutor js = (IJavaScriptExecutor)webDriver;
                    js.ExecuteScript("arguments[0].scrollIntoView();", footer);                    
                }
                
                nextBtn = webDriver.FindElement(By.CssSelector("button[aria-label='Next']"));

                double defaultTimeout = defaultTimeSpan.TotalSeconds;
                _logger.LogTrace("[NextButton] Reverting web drivers timeout back to it's default value {defaultTimeout}", defaultTimeout);
                webDriver.Manage().Timeouts().ImplicitWait = defaultTimeSpan;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("[NextButton] Failed to find next button");
            }

            return nextBtn;
        }

        public HalOperationResult<T> ClickNext<T>(IWebDriver driver) where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            IWebElement nextButton = NextButton(driver);

            if(nextButton == null)
            {
                _logger.LogWarning("[ClickNext]: Next button could not be located");
                return result;
            }

            Stopwatch stopwatch = new();
            try
            {
                RandomWait(1, 4);

                _logger.LogTrace("Clicking next button");
                nextButton.Click();

                Stopwatch sw = Stopwatch.StartNew();
                sw.Start();

                IWebElement searchResultsContainer = default;
                IWebElement noSearchResultsContainer = default;
                _logger.LogTrace("Entering in while loop to ensure we either view the NoSearchResultsContainer or the next page's search results");
                while ((sw.Elapsed.TotalSeconds <= Timeout))
                {
                    try
                    {
                        searchResultsContainer = SearchResultContainerQuery(driver);
                    }
                    catch(Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to locate either search results container or no search results container in the allotted time");
                    }

                    if (searchResultsContainer == null)
                    {
                        // check if error search results page displayed
                        noSearchResultsContainer = NoSearchResultsContainer(driver);
                        if (noSearchResultsContainer != null)
                            break;
                    }
                    else
                    {
                        break;
                    }
                }

                if(noSearchResultsContainer != null)
                {
                    _logger.LogWarning("[ClickNext]: Clicking next displayed search results error page. Attempting to click retry search.");
                    ClickRetrySearch<T>(driver);
                }

            }
            catch(Exception ex)
            {
                _logger.LogWarning(ex, "[ClickNext]: Failed to click next button");
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
                _logger.LogWarning(ex, "[NoSearchResultsContainer] The NoSearchResultsContainer has not been found");
            }

            return noSearchResultsContainer;
        }

        public bool IsNoSearchResultsContainerDisplayed(IWebDriver driver)
        {
            bool isDisplayed = false;
            try
            {
                _logger.LogTrace("[IsNoSearchResultsContainerDisplayed]: Determining if NoSearchResultsContainer is displayed");
                IWebElement noResultsSearchContainer = NoSearchResultsContainer(driver);
                if(noResultsSearchContainer != null)
                {
                    isDisplayed = noResultsSearchContainer.Displayed;
                    _logger.LogTrace("NoSearchResultsContainer element was found. Is it displayed: {isDisplayed}", isDisplayed);                    
                }
            }
            catch(Exception ex)
            {
                _logger.LogWarning(ex, "[IsNoSearchResultsContainerDisplayed]: Web driver error occured locating the element");
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
                _logger.LogWarning(ex, "[RetrySearchButton]: Failed to locate retry search button");
            }
            return retrySearchButton;
        }

        public HalOperationResult<T> ClickRetrySearch<T>(IWebDriver driver) where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            IWebElement retryButton = RetrySearchButton(driver);
            if(retryButton == null)
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
                    if(searchResultContainer == null)
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

        public HalOperationResult<T> SendConnectionRequest<T>(IWebElement prospect) where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            IWebElement actionButton = GetProspectsActionButton(prospect);
            if (actionButton != null)
            {
                if (actionButton.Text == "Connect")
                {
                    RandomWait(1, 7);

                    actionButton.Click();
                }
                else
                {
                    return result;
                }
            }
            else
            {
                return result;
            }

            result.Succeeded = true;
            return result;
        }

        private IWebElement GetProspectsActionButton(IWebElement prospect)
        {
            IWebElement actionButton = default;
            try
            {
                actionButton = prospect.FindElement(By.CssSelector(".entity-result__actions div:not([class='entity-result__actions--empty']"));
                actionButton = actionButton.FindElement(By.TagName("button"));
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Prospect does not contain action button");
            }
            return actionButton;
        }

        private IWebElement CustomizeThisInvitationModal(IWebDriver webDriver)
        {
            IWebElement modal = default;
            try
            {
                modal = webDriver.FindElement(By.CssSelector("#artdeco-modal-outlet .artdeco-modal"));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to locate modal");
            }
            return modal;
        }

        private IWebElement SendNowButton(IWebDriver webDriver)
        {
            IWebElement button = default;
            try
            {
                button = CustomizeThisInvitationModal(webDriver).FindElement(By.CssSelector("button[aria-label='Send now']"));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to locate send now button");
            }
            return button;
        }

        public HalOperationResult<T> ClickSendInModal<T>(IWebDriver webDriver) where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            IWebElement button = SendNowButton(webDriver);
            if(button == null)
            {
                return result;
            }

            RandomWait(1, 8);

            button.Click();

            result.Succeeded = true;
            return result;
        }

        public bool IsNextButtonDisabled(IWebDriver webDriver)
        {
            IWebElement nextButton = NextButton(webDriver);

            if(nextButton == null)
            {
                return false;
            }

            string disabledAttribute = nextButton.GetAttribute("disabled");
            bool.TryParse(disabledAttribute, out bool result);
            return result;
        }
    }
}
