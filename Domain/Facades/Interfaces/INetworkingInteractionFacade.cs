using Domain.Interactions;
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
        bool HandleNoResultsFoundInteraction(InteractionBase interaction);
        bool HandleConnectWithProspectsInteraction(InteractionBase interaction);
        bool HandleGatherProspectsInteraction(InteractionBase interaction);
        bool HandleSearchResultsLimitInteraction(InteractionBase interaction);
    }
}
