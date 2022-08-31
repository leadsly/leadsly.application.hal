namespace Domain.Interactions.ScanProspectsForReplies.CloseAllConversations.Interfaces
{
    public interface ICloseAllConversationsInteractionHandler<TInteraction> : IInteractionHandler<TInteraction>
        where TInteraction : IInteraction
    {
    }
}
