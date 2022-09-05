using Domain.Interactions.Networking.GatherProspects.Interfaces;
using Domain.Models.ProspectList;
using OpenQA.Selenium;
using System.Collections.Generic;

namespace Domain.Interactions.Networking.Decorators
{
    public class RetryGatherProspectsHandlerDecorator : IGatherProspectsInteractionHandler
    {
        public List<PersistPrimaryProspect> PersistPrimaryProspects => _decorated.PersistPrimaryProspects;

        public IList<IWebElement> Prospects => _decorated.Prospects;

        public RetryGatherProspectsHandlerDecorator(IGatherProspectsInteractionHandler decorated)
        {
            _decorated = decorated;
        }

        private readonly IGatherProspectsInteractionHandler _decorated;

        public bool HandleInteraction(InteractionBase interaction)
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
