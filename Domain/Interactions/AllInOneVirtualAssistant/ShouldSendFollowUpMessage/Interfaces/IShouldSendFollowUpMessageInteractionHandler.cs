using Domain.Models.DeepScanProspectsForReplies;
using OpenQA.Selenium;

namespace Domain.Interactions.AllInOneVirtualAssistant.ShouldSendFollowUpMessage.Interfaces
{
    public interface IShouldSendFollowUpMessageInteractionHandler : IInteractionHandler
    {
        public bool DidProspectReply { get; }
        public IWebElement PopupConversation { get; }
        public ProspectRepliedModel GetProspect();
    }
}
