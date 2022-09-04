using Domain.Interactions.Networking.SearchResultsLimit.Interfaces;
using Domain.POMs.Pages;
using Domain.Services.Interfaces;
using Domain.Services.Interfaces.POMs;
using Microsoft.Extensions.Logging;
using System;

namespace Domain.Interactions.Networking.SearchResultsLimit
{
    public class SearchResultsLimitInteractionHandler : ISearchResultsLimitInteractionHandler
    {
        public SearchResultsLimitInteractionHandler(ILogger<SearchResultsLimitInteractionHandler> logger, ILinkedInSearchPage linkedInSearchPage, ISearchPageFooterService searchPageFooterService, IHumanBehaviorService humanBehaviorService)
        {
            _logger = logger;
            _searchPageFooterService = searchPageFooterService;
            _linkedInSearchPage = linkedInSearchPage;
            _humanBehaviorService = humanBehaviorService;
        }

        private readonly ISearchPageFooterService _searchPageFooterService;
        private readonly ILinkedInSearchPage _linkedInSearchPage;
        private readonly IHumanBehaviorService _humanBehaviorService;
        private readonly ILogger<SearchResultsLimitInteractionHandler> _logger;

        /// <summary>
        /// Returns true if monthly search limit has been reached, otherwise returns false
        /// Tell tale signs that monthly search limit has been reached.
        /// 1. The number of results is truncated from 100 to whatever page was next
        /// 2. There are three results on this page and then again three results on the previous page
        /// 3. Each time you click previous button the number of total results shrinks from previous by one
        /// 4. The next button is disabled if it is not page 100
        /// </summary>
        /// <param name="interaction"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public bool HandleInteraction(InteractionBase interaction)
        {
            SearchResultsLimitInteraction searchResultsLimitInteraction = interaction as SearchResultsLimitInteraction;
            // if search results paginator is not displayed we're on page one and we've reached monthly limit
            if (_linkedInSearchPage.IsSearchResultsPaginationDisplayed(searchResultsLimitInteraction.WebDriver) == false)
            {
                _logger.LogDebug("Monthly search limit has been reached");
                return true;
            }

            int? totalResults = _searchPageFooterService.GetTotalResults(searchResultsLimitInteraction.WebDriver);
            if (totalResults.HasValue)
            {
                int previousTotalResults = totalResults.Value;
                if (_linkedInSearchPage.IsPreviousButtonClickable(searchResultsLimitInteraction.WebDriver) == true)
                {
                    bool? goToPreviousPageOperation = _searchPageFooterService.GoToThePreviousPage(searchResultsLimitInteraction.WebDriver);
                    if (goToPreviousPageOperation == null || goToPreviousPageOperation == false)
                    {
                        _logger.LogDebug("Failed to navigate to the previous page");

                        return false;
                    }

                    if (_linkedInSearchPage.IsSearchResultsPaginationDisplayed(searchResultsLimitInteraction.WebDriver) == false)
                    {
                        _logger.LogDebug("Monthly search limit has been reached");
                        return true;
                    }

                    totalResults = _searchPageFooterService.GetTotalResults(searchResultsLimitInteraction.WebDriver);
                    if (totalResults.HasValue)
                    {
                        if (totalResults.Value < previousTotalResults)
                        {
                            _logger.LogDebug("Monthly search limit has been reached");
                            return true;
                        }
                        else
                        {
                            _logger.LogDebug("Monthly search limit has not been reached. Navigating back forward to be on the current page.");
                            // bring back webdriver to the current page
                            _searchPageFooterService.GoToTheNextPage(searchResultsLimitInteraction.WebDriver);
                        }
                    }
                    else
                    {
                        _logger.LogDebug("Failed to get total results");
                        return false;
                    }
                }
            }
            else
            {
                _logger.LogDebug("Failed to get total results");
                return false;
            }

            return false;
        }
    }
}
