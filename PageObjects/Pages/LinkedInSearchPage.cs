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
    public class LinkedInSearchPage : ILinkedInSearchPage
    {
        public LinkedInSearchPage(ILogger<LinkedInSearchPage> logger)
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
                _logger.LogInformation("Getting last page of the total search results from the hitlist");
                IWebElement ul = SearchResultFooter(webDriver).FindElement(By.CssSelector(".artdeco-pagination .artdeco-pagination__pages"));
                lis = ul.FindElements(By.TagName("li")).ToList();
                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to locate last page in the navigational search result");
            }

            return lis.LastOrDefault();
        }

        private Func<IWebDriver, IWebElement> SearchResultContainerQuery = (webDriver) => webDriver.FindElement(By.CssSelector("#main .search-results-container"));

        private IWebElement SearchResultsContainer(IWebDriver webDriver)
        {
            IWebElement searchResultContainer = default;
            try
            {
                _logger.LogInformation("Temporarily changing web driver's implicit wait time to {Timeout}", Timeout);
                TimeSpan defaultTimeSpan = webDriver.Manage().Timeouts().ImplicitWait;
                webDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(Timeout);

                // searchResultContainer = webDriver.FindElement(By.CssSelector("#main .search-results-container"));
                searchResultContainer = SearchResultContainerQuery(webDriver);

                double defaultTimeout = defaultTimeSpan.TotalSeconds;
                _logger.LogInformation("Reverting web drivers timeout back to it's default value {defaultTimeout}", defaultTimeout);
                webDriver.Manage().Timeouts().ImplicitWait = defaultTimeSpan;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to locate search results container");
            }

            return searchResultContainer;
        }

        private IWebElement HitList(IWebDriver webDriver)
        {
            IWebElement hitList = default;
            try
            {
                _logger.LogInformation("Temporarily changing web driver's implicit wait time to {Timeout}", Timeout);
                TimeSpan defaultTimeSpan = webDriver.Manage().Timeouts().ImplicitWait;
                webDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(Timeout);

                hitList = SearchResultsContainer(webDriver).FindElement(By.CssSelector("div"));

                double defaultTimeout = defaultTimeSpan.TotalSeconds;
                _logger.LogInformation("Reverting web drivers timeout back to it's default value {defaultTimeout}", defaultTimeout);
                webDriver.Manage().Timeouts().ImplicitWait = defaultTimeSpan;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to locate search hitlist");
            }

            return hitList;
        }

        private IWebElement ProspectList(IWebDriver webDriver)
        {
            IWebElement propsectListUlNode = default;
            try
            {
                _logger.LogInformation("[ProspectList] Temporarily changing web driver's implicit wait time to {Timeout}", Timeout);
                TimeSpan defaultTimeSpan = webDriver.Manage().Timeouts().ImplicitWait;
                webDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(Timeout);

                propsectListUlNode = HitList(webDriver).FindElement(By.TagName("ul"));

                double defaultTimeout = defaultTimeSpan.TotalSeconds;
                _logger.LogInformation("[ProspectList] Reverting web drivers timeout back to it's default value {defaultTimeout}", defaultTimeout);
                webDriver.Manage().Timeouts().ImplicitWait = defaultTimeSpan;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ProspectList] Failed to locate the propsect list 'ul' node");
            }

            return propsectListUlNode;
        }

        private List<IWebElement> ProspectsAsWebElements(IWebDriver webDriver)
        {
            List<IWebElement> prospects = default;
            try
            {
                _logger.LogInformation("[ProspectsAsWebElements] Temporarily changing web driver's implicit wait time to {Timeout}", Timeout);
                TimeSpan defaultTimeSpan = webDriver.Manage().Timeouts().ImplicitWait;
                webDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(Timeout);

                prospects = ProspectList(webDriver).FindElements(By.TagName("li")).ToList();

                double defaultTimeout = defaultTimeSpan.TotalSeconds;
                _logger.LogInformation("[ProspectsAsWebElements] Reverting web drivers timeout back to it's default value {defaultTimeout}", defaultTimeout);
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
                _logger.LogInformation("[NextButton] Temporarily changing web driver's implicit wait time to {Timeout}", Timeout);
                TimeSpan defaultTimeSpan = webDriver.Manage().Timeouts().ImplicitWait;
                webDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(Timeout);

                IWebElement footer = webDriver.FindElement(By.CssSelector(".global-footer.global-footer--static"));

                if (footer == null)
                {
                    _logger.LogInformation("[NextButton] Footer could not be located. Returning null");
                    return null;
                }
                else
                {
                    _logger.LogInformation("[NextButton] Executing javascript 'scrollIntoView' to scroll footer into view");
                    IJavaScriptExecutor js = (IJavaScriptExecutor)webDriver;
                    js.ExecuteScript("arguments[0].scrollIntoView();", footer);                    
                }
                
                nextBtn = webDriver.FindElement(By.CssSelector("button[aria-label='Next']"));

                double defaultTimeout = defaultTimeSpan.TotalSeconds;
                _logger.LogInformation("[NextButton] Reverting web drivers timeout back to it's default value {defaultTimeout}", defaultTimeout);
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

            Random random = new Random();
            Stopwatch stopwatch = new();
            try
            {
                Thread.Sleep(1000);
                nextButton.Click();

                Stopwatch sw = Stopwatch.StartNew();
                sw.Start();

                IWebElement searchResultsContainer = default;
                IWebElement noSearchResultsContainer = default;
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
                _logger.LogInformation("[IsNoSearchResultsContainerDisplayed]: Determining if NoSearchResultsContainer is displayed");
                IWebElement noResultsSearchContainer = NoSearchResultsContainer(driver);
                if(noResultsSearchContainer != null)
                {
                    isDisplayed = noResultsSearchContainer.Displayed;
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
                _logger.LogInformation("[ClickRetrySearch] Clicking retry search results because we've gotten an error page");
                retryButton.Click();

                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
                IWebElement searchResults = wait.Until<IWebElement>(webDriver =>
                {
                    _logger.LogInformation("[ClickRetrySearch] locating search results container");
                    IWebElement searchResultContainer = SearchResultContainerQuery(webDriver);
                    if(searchResultContainer == null)
                    {
                        _logger.LogInformation("[ClickRetrySearch] Failed to locate search results container. Attemping to click retry button again");
                        retryButton.Click();
                    }
                    return searchResultContainer;
                });

                if (searchResults == null)
                {
                    _logger.LogInformation("[ClickRetrySearch] Search results container is null");
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
                    actionButton.Click();
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
    }
}
