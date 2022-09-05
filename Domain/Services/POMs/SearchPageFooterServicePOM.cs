using Domain.POMs;
using Domain.POMs.Controls;
using Domain.POMs.Pages;
using Domain.Services.Interfaces;
using Domain.Services.Interfaces.POMs;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;

namespace Domain.Services.POMs
{
    public class SearchPageFooterServicePOM : ISearchPageFooterServicePOM
    {
        public SearchPageFooterServicePOM(IHumanBehaviorService humanBehaviorService, ISearchResultsFooter searchResultsFooter, ILinkedInSearchPage linkedInSearchPage, ILogger<SearchPageFooterServicePOM> logger)
        {
            _humanBehaviorService = humanBehaviorService;
            _linkedInSearchPage = linkedInSearchPage;
            _logger = logger;
            _searchResultsFooter = searchResultsFooter;
        }

        private readonly ISearchResultsFooter _searchResultsFooter;
        private readonly ILinkedInSearchPage _linkedInSearchPage;
        private readonly IHumanBehaviorService _humanBehaviorService;
        private readonly ILogger<SearchPageFooterServicePOM> _logger;

        public int? GetTotalResults(IWebDriver webDriver, bool scrollTop = false)
        {
            // get total searchresults for this url
            _linkedInSearchPage.ScrollFooterIntoView(webDriver);
            _humanBehaviorService.RandomWaitSeconds(1, 2);

            // get the total number of search results            
            int? totalCount = _searchResultsFooter.GetTotalSearchResults(webDriver);
            if (totalCount == null)
            {
                return null;
            }

            IWebElement linkedInlogoFooter = _searchResultsFooter.LinkInFooterLogoIcon(webDriver);
            _humanBehaviorService.RandomClickElement(linkedInlogoFooter);
            _humanBehaviorService.RandomWaitMilliSeconds(500, 800);

            if (scrollTop == true)
            {
                webDriver.ScrollTop(_humanBehaviorService);
            }

            return totalCount;
        }

        public bool? IsLastPage(IWebDriver webDriver, bool verifyWithWebDriver = false, int? currentPage = null, int? totalResultCount = null)
        {
            bool? isLastPage = false;
            if (currentPage != null && totalResultCount != null)
            {
                _logger.LogDebug("CurrentPage and TotalResultCount parameters were provided");

                if (verifyWithWebDriver == false)
                {
                    return currentPage == totalResultCount;
                }
                else
                {
                    _logger.LogDebug("CurrentPage {0} equals TotalResultCount {1}", currentPage, totalResultCount);
                    // ensuring we are on the last page
                    _logger.LogDebug("Checking if the next button is disabled. To Ensure we are on the last page");
                    _linkedInSearchPage.ScrollFooterIntoView(webDriver);
                    _humanBehaviorService.RandomWaitSeconds(1, 2);

                    bool? isNextbtnClickable = _searchResultsFooter.IsNextButtonClickable(webDriver);
                    if (isNextbtnClickable == null)
                    {
                        _logger.LogDebug("Unable to determine if the next button is clickable");
                        isLastPage = null;
                    }
                    else if (isNextbtnClickable == false)
                    {
                        _logger.LogDebug("Next button is disabled");
                        isLastPage = true;
                    }
                    else
                    {
                        _logger.LogDebug("Next button is not disabled");
                        isLastPage = false;
                    }

                    _logger.LogDebug("Ensuring that WebDriver result of IsLastPage matches currentPage and totalResultCount");
                    if ((currentPage == totalResultCount) && (isLastPage == true))
                    {
                        _logger.LogDebug("WebDriver result of IsLastPage matches currentPage and totalResultCount");
                    }
                    else
                    {
                        _logger.LogDebug("WebDriver result of IsLastPage does not match currentPage and totalResultCount. Going with condition that checks if CurrentPage {0} is equal to TotalSearchResults {1}", currentPage, totalResultCount);
                        isLastPage = currentPage == totalResultCount;
                    }
                }
            }

            return isLastPage;
        }

        public bool? GoToTheNextPage(IWebDriver webDriver)
        {
            bool? succeeded = false;

            if (_linkedInSearchPage.ScrollFooterIntoView(webDriver) == false)
            {
                _logger.LogError("Failed to scroll footer into view");
                succeeded = false;
            }

            IWebElement linkedInFooterLogo = _searchResultsFooter.LinkInFooterLogoIcon(webDriver);
            _humanBehaviorService.RandomClickElement(linkedInFooterLogo);
            _humanBehaviorService.RandomWaitMilliSeconds(2000, 5000);


            bool? clickingNextBtnSucceeded = _searchResultsFooter.ClickNext(webDriver);
            if (clickingNextBtnSucceeded == null)
            {
                _logger.LogError("Failed to locate next button on the page");
                succeeded = null;
            }

            bool searchResultFinishedLoading = _linkedInSearchPage.WaitUntilSearchResultsFinishedLoading(webDriver);
            if (searchResultFinishedLoading == false)
            {
                _logger.LogError("Search results never finished loading.");
                succeeded = false;
            }
            else
            {
                _logger.LogDebug("Search results finished loading successfully.");
                succeeded = true;
            }

            return succeeded;
        }

        public bool? IsNextButtonClickable(IWebDriver webDriver)
        {
            return _searchResultsFooter.IsNextButtonClickable(webDriver);
        }

        public bool? IsPreviousButtonClickable(IWebDriver webDriver)
        {
            return _searchResultsFooter.IsPreviousButtonClickable(webDriver);
        }

        public bool? GoToThePreviousPage(IWebDriver webDriver)
        {
            bool? succeeded = false;
            if (_linkedInSearchPage.ScrollFooterIntoView(webDriver) == false)
            {
                _logger.LogError("Failed to scroll footer into view");
                succeeded = false;
            }

            IWebElement linkedInFooterLogo = _searchResultsFooter.LinkInFooterLogoIcon(webDriver);
            _humanBehaviorService.RandomClickElement(linkedInFooterLogo);
            _humanBehaviorService.RandomWaitMilliSeconds(2000, 5000);


            bool? clickingPreviousBtnSucceeded = _searchResultsFooter.ClickPrevious(webDriver);
            if (clickingPreviousBtnSucceeded == null)
            {
                _logger.LogError("Failed to locate previous button on the page");
                succeeded = null;
            }

            bool searchResultFinishedLoading = _linkedInSearchPage.WaitUntilSearchResultsFinishedLoading(webDriver);
            if (searchResultFinishedLoading == false)
            {
                _logger.LogError("Search results never finished loading.");
                succeeded = false;
            }
            else
            {
                _logger.LogDebug("Search results finished loading successfully.");
                succeeded = true;
            }

            return succeeded;
        }
    }
}
