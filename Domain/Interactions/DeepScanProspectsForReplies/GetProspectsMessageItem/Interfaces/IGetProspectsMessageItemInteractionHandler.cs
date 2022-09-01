using OpenQA.Selenium;
using System.Collections.Generic;

namespace Domain.Interactions.DeepScanProspectsForReplies.GetProspectsMessageItem.Interfaces
{
    public interface IGetProspectsMessageItemInteractionHandler<TInteraction> : IInteractionHandler<TInteraction>
        where TInteraction : IInteraction
    {
        public IList<IWebElement> GetProspectMessageItems();
    }
}
