using Domain.Interactions.Networking.GatherProspects;
using Domain.Interactions.Networking.GatherProspects.Interfaces;
using Domain.Models.Requests;
using OpenQA.Selenium;
using System.Collections.Generic;

namespace Domain.Interactions.Networking.Decorators
{
    public class RetryGatherProspectsHandlerDecorator<TInteraction> : IGatherProspectsInteractionHandler<TInteraction>
        where TInteraction : GatherProspectsInteraction
    {
        public List<PersistPrimaryProspectRequest> PersistPrimaryProspectRequests => _decorated.PersistPrimaryProspectRequests;

        public IList<IWebElement> Prospects => _decorated.Prospects;

        public RetryGatherProspectsHandlerDecorator(IGatherProspectsInteractionHandler<TInteraction> decorated)
        {
            _decorated = decorated;
        }

        private readonly IGatherProspectsInteractionHandler<TInteraction> _decorated;

        public bool HandleInteraction(TInteraction interaction)
        {
            bool succeeded = _decorated.HandleInteraction(interaction);
            if (succeeded == false)
            {
                // do something if it fails. We could refresh the page here and re try again.
            }

            return succeeded;
        }
    }
}
