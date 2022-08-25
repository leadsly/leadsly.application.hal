using Domain.Models.Requests;
using OpenQA.Selenium;
using System.Collections.Generic;

namespace Domain.Interactions.Networking.GatherProspects.Interfaces
{
    public interface IGatherProspectsInteractionHandler<TInteraction> : IInteractionHandler<TInteraction> 
        where TInteraction : IInteraction
    {
        public List<PersistPrimaryProspectRequest> PersistPrimaryProspectRequests { get; }
        public IList<IWebElement> Prospects { get; }        
    }
}
