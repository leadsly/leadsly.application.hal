using Domain.Interactions.Networking.ConnectWithProspect;
using Domain.Interactions.Networking.GatherProspects;
using Domain.Interactions.Networking.NoResultsFound;
using Domain.Interactions.Networking.SearchResultsLimit;
using Domain.Models.Requests;
using OpenQA.Selenium;
using System.Collections.Generic;

namespace Domain.Facades.Interfaces
{
    public interface INetworkingInteractionFacade
    {
        public List<PersistPrimaryProspectRequest> PersistPrimaryProspectRequests { get; }
        public IList<IWebElement> Prospects { get; }
        public ConnectionSentRequest ConnectionSentRequest { get; }
        bool HandleInteraction(NoResultsFoundInteraction interaction);
        bool HandleInteraction(ConnectWithProspectInteraction interaction);
        bool HandleInteraction(GatherProspectsInteraction interaction);
        bool HandleInteraction(SearchResultsLimitInteraction interaction);
    }
}
