namespace Domain.Interactions.DeepScanProspectsForReplies.GetAllVisibleConversationCount.Interfaces
{
    public interface IGetAllVisibleConversationCountInteractionHandler : IInteractionHandler
    {
        int GetConversationCount();
    }
}
