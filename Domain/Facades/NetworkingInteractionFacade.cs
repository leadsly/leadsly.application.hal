using Domain.Facades.Interfaces;
using Domain.Interactions;
using Domain.Interactions.Networking.Decorators;
using Domain.Interactions.Networking.GetTotalSearchResults.Interfaces;
using Domain.Interactions.Networking.GoToTheNextPage.Interfaces;
using Domain.Interactions.Networking.IsLastPage.Interfaces;
using Domain.Interactions.Networking.IsNextButtonDisabled.Interfaces;
using Domain.Interactions.Networking.NoResultsFound.Interfaces;
using Domain.Interactions.Networking.SearchResultsLimit.Interfaces;
using Domain.Models.ProspectList;
using Domain.Models.SendConnections;
using OpenQA.Selenium;
using System.Collections.Generic;

namespace Domain.Facades
{
    public class NetworkingInteractionFacade : INetworkingInteractionFacade
    {
        public NetworkingInteractionFacade(RetryGatherProspectsHandlerDecorator gatherProspectsInteractionHandler,
            RetryConnectWithProspectHandlerDecorator connectWithProspectInteractionHandler,
            ISearchResultsLimitInteractionHandler searchResultsLimitHandler,
            INoResultsFoundInteractionHandler noSearchResultsHandler,
            IGetTotalSearchResultsInteractionHandler getTotalSearchResultsHandler,
            IIsLastPageInteractionHandler isLastPageHandler,
            IGoToTheNextPageInteractionHandler goToTheNextPageHandler,
            IIsNextButtonDisabledInteractionHandler isNextButtonDisabledHandler)
        {
            _searchResultsLimitHandler = searchResultsLimitHandler;
            _noSearchResultsHandler = noSearchResultsHandler;
            _connectWithProspectInteractionHandler = connectWithProspectInteractionHandler;
            _gatherProspectsInteractionHandler = gatherProspectsInteractionHandler;
            _getTotalSearchResultsHandler = getTotalSearchResultsHandler;
            _isLastPageHandler = isLastPageHandler;
            _goToTheNextPageHandler = goToTheNextPageHandler;
            _isNextButtonDisabledHandler = isNextButtonDisabledHandler;
        }

        private readonly ISearchResultsLimitInteractionHandler _searchResultsLimitHandler;
        private readonly INoResultsFoundInteractionHandler _noSearchResultsHandler;
        private readonly RetryConnectWithProspectHandlerDecorator _connectWithProspectInteractionHandler;
        private readonly RetryGatherProspectsHandlerDecorator _gatherProspectsInteractionHandler;
        private readonly IGetTotalSearchResultsInteractionHandler _getTotalSearchResultsHandler;
        private readonly IIsLastPageInteractionHandler _isLastPageHandler;
        private readonly IGoToTheNextPageInteractionHandler _goToTheNextPageHandler;
        private readonly IIsNextButtonDisabledInteractionHandler _isNextButtonDisabledHandler;

        public List<PersistPrimaryProspectModel> PersistPrimaryProspects => _gatherProspectsInteractionHandler.PersistPrimaryProspects;

        public IList<IWebElement> Prospects => _gatherProspectsInteractionHandler.Prospects;

        public ConnectionSentModel ConnectionSent => _connectWithProspectInteractionHandler.ConnectionSent;

        public int TotalNumberOfSearchResults => _getTotalSearchResultsHandler.GetTotalResults();

        public bool ErrorToastMessageDetected => _connectWithProspectInteractionHandler.ErrorToastMessageDetected;

        public bool HandleNoResultsFoundInteraction(InteractionBase interaction)
        {
            return _noSearchResultsHandler.HandleInteraction(interaction);
        }

        public bool HandleConnectWithProspectsInteraction(InteractionBase interaction)
        {
            return _connectWithProspectInteractionHandler.HandleInteraction(interaction);
        }

        public bool HandleGatherProspectsInteraction(InteractionBase interaction)
        {
            return _gatherProspectsInteractionHandler.HandleInteraction(interaction);
        }

        public bool HandleSearchResultsLimitInteraction(InteractionBase interaction)
        {
            return _searchResultsLimitHandler.HandleInteraction(interaction);
        }

        public bool HandleGetTotalNumberOfSearchResults(InteractionBase interaction)
        {
            return _getTotalSearchResultsHandler.HandleInteraction(interaction);
        }

        public bool HandleIsLastPageInteraction(InteractionBase interaction)
        {
            return _isLastPageHandler.HandleInteraction(interaction);
        }

        public bool HandleGoToTheNextPageInteraction(InteractionBase interaction)
        {
            return _goToTheNextPageHandler.HandleInteraction(interaction);
        }

        public bool HandleCheckIfNextButtonIsNullOrDisabledInteraction(InteractionBase interaction)
        {
            return _isNextButtonDisabledHandler.HandleInteraction(interaction);
        }
    }
}
