namespace Domain.Interactions.DeepScanProspectsForReplies.GetProspectsMessageItem.Interfaces
{
    public interface IGetProspectsMessageItemInteractionHandler<TInteraction> : IInteractionHandler<TInteraction>
        where TInteraction : IInteraction
    {
    }
}
