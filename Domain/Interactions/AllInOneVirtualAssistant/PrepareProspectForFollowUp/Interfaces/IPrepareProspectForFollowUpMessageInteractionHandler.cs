using OpenQA.Selenium;

namespace Domain.Interactions.AllInOneVirtualAssistant.PrepareProspectForFollowUp.Interfaces
{
    public interface IPrepareProspectForFollowUpMessageInteractionHandler : IInteractionHandler
    {
        public IWebElement PopupConversation { get; }
    }
}
