using Domain.Facades.Interfaces;
using Domain.Interactions.Networking.ConnectWithProspect;
using Domain.Interactions.Networking.Decorators;
using Domain.Interactions.Networking.GatherProspects;
using Domain.Interactions.Networking.NoResultsFound;
using Domain.Interactions.Networking.NoResultsFound.Interfaces;
using Domain.Models.Requests;
using OpenQA.Selenium;
using System.Collections.Generic;

namespace Domain.Facades
{
    public class NetworkingInteractionFacade : INetworkingInteractionFacade
    {
        public NetworkingInteractionFacade(RetryGatherProspectsHandlerDecorator<GatherProspectsInteraction> gatherProspectsInteractionHandler,
            RetryConnectWithProspectHandlerDecorator<ConnectWithProspectInteraction> connectWithProspectInteractionHandler,
            INoResultsFoundInteractionHandler<NoResultsFoundInteraction> noSearchResultsHandler)
        {
            _noSearchResultsHandler = noSearchResultsHandler;
            _connectWithProspectInteractionHandler = connectWithProspectInteractionHandler;
            _gatherProspectsInteractionHandler = gatherProspectsInteractionHandler;
        }

        private readonly INoResultsFoundInteractionHandler<NoResultsFoundInteraction> _noSearchResultsHandler;
        private readonly RetryConnectWithProspectHandlerDecorator<ConnectWithProspectInteraction> _connectWithProspectInteractionHandler;
        private readonly RetryGatherProspectsHandlerDecorator<GatherProspectsInteraction> _gatherProspectsInteractionHandler;

        public List<PersistPrimaryProspectRequest> PersistPrimaryProspectRequests => _gatherProspectsInteractionHandler.PersistPrimaryProspectRequests;

        public IList<IWebElement> Prospects => _gatherProspectsInteractionHandler.Prospects;

        public ConnectionSentRequest ConnectionSentRequest => _connectWithProspectInteractionHandler.ConnectionSentRequest;

        public bool HandleInteraction(NoResultsFoundInteraction interaction)
        {
            return _noSearchResultsHandler.HandleInteraction(interaction);
        }

        public bool HandleInteraction(ConnectWithProspectInteraction interaction)
        {
            return _connectWithProspectInteractionHandler.HandleInteraction(interaction);
        }

        public bool HandleInteraction(GatherProspectsInteraction interaction)
        {
            return _gatherProspectsInteractionHandler.HandleInteraction(interaction);
        }
    }
}
