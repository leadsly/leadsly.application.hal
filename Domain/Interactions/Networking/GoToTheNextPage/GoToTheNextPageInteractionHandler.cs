using Domain.Interactions.Networking.GoToTheNextPage.Interfaces;
using Domain.Interactions.Networking.NoResultsFound;
using Domain.Interactions.Networking.NoResultsFound.Interfaces;
using Domain.Services.Interfaces.POMs;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;

namespace Domain.Interactions.Networking.GoToTheNextPage
{
    public class GoToTheNextPageInteractionHandler : IGoToTheNextPageInteractionHandler
    {
        public GoToTheNextPageInteractionHandler(
            ILogger<GoToTheNextPageInteractionHandler> logger,
            INoResultsFoundInteractionHandler noSearchResultsHandler,
            ISearchPageFooterServicePOM service)
        {
            _logger = logger;
            _service = service;
            _noSearchResultsHandler = noSearchResultsHandler;
        }

        private readonly ILogger<GoToTheNextPageInteractionHandler> _logger;
        private readonly ISearchPageFooterServicePOM _service;
        private readonly INoResultsFoundInteractionHandler _noSearchResultsHandler;

        public bool HandleInteraction(InteractionBase interaction)
        {
            GoToTheNextPageInteraction goToTheNextInteraction = interaction as GoToTheNextPageInteraction;
            IWebDriver webDriver = goToTheNextInteraction.WebDriver;

            bool succeeded = false;
            if (NavigateToNextPage(webDriver) == false)
            {
                succeeded = false;
            }
            else
            {
                succeeded = true;
            }

            return succeeded;
        }

        private bool NavigateToNextPage(IWebDriver webDriver)
        {
            bool succeeded = false;
            bool? goToNextPageOperation = _service.GoToTheNextPage(webDriver);
            if (goToNextPageOperation == null || goToNextPageOperation == false)
            {
                _logger.LogDebug("Navigation to the next page failed. Breaking out of this loop and updating search page url progress");
                succeeded = false;
            }
            else
            {
                if (NoSearchResultsDisplayed(webDriver) == true)
                {
                    succeeded = false;
                }
                else
                {
                    succeeded = true;
                }
            }

            return succeeded;
        }

        private bool NoSearchResultsDisplayed(IWebDriver webDriver)
        {
            NoResultsFoundInteraction interaction = new()
            {
                WebDriver = webDriver
            };

            if (_noSearchResultsHandler.HandleInteraction(interaction))
            {
                _logger.LogError("No search results page displayed again. This means we've tried refreshing and waiting for results page to be displayed, but it wasn't.");
                return true;
            }
            return false;
        }
    }
}
