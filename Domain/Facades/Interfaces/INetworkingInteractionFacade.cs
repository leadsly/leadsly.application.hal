using Domain.Interactions;
using Domain.Models.ProspectList;
using Domain.Models.SendConnections;
using OpenQA.Selenium;
using System.Collections.Generic;

namespace Domain.Facades.Interfaces
{
    public interface INetworkingInteractionFacade
    {
        public List<PersistPrimaryProspectModel> PersistPrimaryProspects { get; }
        public IList<IWebElement> Prospects { get; }
        public ConnectionSentModel ConnectionSent { get; }
        bool HandleNoResultsFoundInteraction(InteractionBase interaction);
        bool HandleConnectWithProspectsInteraction(InteractionBase interaction);
        bool HandleGatherProspectsInteraction(InteractionBase interaction);
        bool HandleSearchResultsLimitInteraction(InteractionBase interaction);
    }
}
