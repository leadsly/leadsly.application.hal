using Domain.Facades.Interfaces;
using Domain.Interactions;
using Domain.Interactions.Networking.Decorators;
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
            INoResultsFoundInteractionHandler noSearchResultsHandler)
        {
            _searchResultsLimitHandler = searchResultsLimitHandler;
            _noSearchResultsHandler = noSearchResultsHandler;
            _connectWithProspectInteractionHandler = connectWithProspectInteractionHandler;
            _gatherProspectsInteractionHandler = gatherProspectsInteractionHandler;
        }

        private readonly ISearchResultsLimitInteractionHandler _searchResultsLimitHandler;
        private readonly INoResultsFoundInteractionHandler _noSearchResultsHandler;
        private readonly RetryConnectWithProspectHandlerDecorator _connectWithProspectInteractionHandler;
        private readonly RetryGatherProspectsHandlerDecorator _gatherProspectsInteractionHandler;

        public List<PersistPrimaryProspect> PersistPrimaryProspects => _gatherProspectsInteractionHandler.PersistPrimaryProspects;

        public IList<IWebElement> Prospects => _gatherProspectsInteractionHandler.Prospects;

        public ConnectionSent ConnectionSent => _connectWithProspectInteractionHandler.ConnectionSent;

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
    }
}
