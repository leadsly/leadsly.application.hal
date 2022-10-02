using Domain.Models.DeepScanProspectsForReplies;

namespace Domain.Interactions.AllInOneVirtualAssistant.ShouldSendFollowUpMessage.Interfaces
{
    public interface IShouldSendFollowUpMessageInteractionHandler : IInteractionHandler
    {
        public bool DidProspectReply { get; }
        public ProspectRepliedModel GetProspect();
    }
}
