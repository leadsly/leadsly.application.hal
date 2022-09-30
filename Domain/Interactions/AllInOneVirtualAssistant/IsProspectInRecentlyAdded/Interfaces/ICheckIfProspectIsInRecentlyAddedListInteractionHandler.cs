using OpenQA.Selenium;

namespace Domain.Interactions.AllInOneVirtualAssistant.IsProspectInRecentlyAdded.Interfaces
{
    public interface ICheckIfProspectIsInRecentlyAddedListInteractionHandler : IInteractionHandler
    {
        public IWebElement ProspectFromRecentlyAdded { get; }
    }
}
