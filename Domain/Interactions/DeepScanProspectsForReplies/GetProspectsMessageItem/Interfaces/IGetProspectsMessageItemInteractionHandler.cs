using OpenQA.Selenium;
using System.Collections.Generic;

namespace Domain.Interactions.DeepScanProspectsForReplies.GetProspectsMessageItem.Interfaces
{
    public interface IGetProspectsMessageItemInteractionHandler : IInteractionHandler
    {
        public IList<IWebElement> GetProspectMessageItems();
    }
}
