using Domain.Interactions.Networking.NoResultsFound.Interfaces;
using Domain.POMs.Pages;
using Domain.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Domain.Interactions.Networking.NoResultsFound
{
    public class NoResultsFoundInteractionHandler : INoResultsFoundInteractionHandler
    {
        public NoResultsFoundInteractionHandler(ILogger<NoResultsFoundInteractionHandler> logger, ILinkedInSearchPage linkedInSearchPage, IHumanBehaviorService humanBehaviorService)
        {
            _logger = logger;
            _linkedInSearchPage = linkedInSearchPage;
            _humanBehaviorService = humanBehaviorService;
        }

        private readonly ILinkedInSearchPage _linkedInSearchPage;
        private readonly IHumanBehaviorService _humanBehaviorService;
        private readonly ILogger<NoResultsFoundInteractionHandler> _logger;

        /// <summary>
        /// Returns true if NoSearchResults container is found, else returns false
        /// </summary>
        /// <param name="interaction"></param>
        /// <returns></returns>
        public bool HandleInteraction(InteractionBase interaction)
        {
            NoResultsFoundInteraction noResultsFoundInteraction = interaction as NoResultsFoundInteraction;
            bool finishedLoading = _linkedInSearchPage.WaitUntilSearchResultsFinishedLoading(noResultsFoundInteraction.WebDriver);
            if (finishedLoading == true)
            {
                SearchResultsPageResult searchPageResult = _linkedInSearchPage.DetermineSearchResultsPage(noResultsFoundInteraction.WebDriver);
                if (searchPageResult == SearchResultsPageResult.Results)
                {
                    _logger.LogDebug("Search results found as expected");
                    return false;
                }
                else if (searchPageResult == SearchResultsPageResult.NoResults)
                {
                    _logger.LogDebug("No search results page was displayed. Waiting for a few (3 to 6 seconds) seconds then refreshing the page to see if results page is found");
                    _humanBehaviorService.RandomWaitSeconds(3, 6);
                    noResultsFoundInteraction.WebDriver.Navigate().Refresh();
                    _logger.LogDebug("Finished refreshing the page");
                    finishedLoading = _linkedInSearchPage.WaitUntilSearchResultsFinishedLoading(noResultsFoundInteraction.WebDriver);
                    _logger.LogDebug("Waiting until results finished loading");
                    if (finishedLoading == false)
                    {
                        _logger.LogDebug("Web page failed to finish loading in the allowed time.");
                        return false;
                    }

                    searchPageResult = _linkedInSearchPage.DetermineSearchResultsPage(noResultsFoundInteraction.WebDriver);
                    if (searchPageResult == SearchResultsPageResult.NoResults)
                    {
                        _logger.LogDebug("Failed to view search results page");
                        return true;
                    }

                    return false;
                }
                else
                {
                    _logger.LogDebug("SearchPageResults value is Unknown");
                    return false;
                }
            }

            return false;
        }
    }
}
