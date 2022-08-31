namespace Domain.Interactions.DeepScanProspectsForReplies.ClearMessagingSearchCriteria.Interfaces
{
    public interface IClearMessagingSearchCriteriaInteractionHandler<TInteraction> : IInteractionHandler<TInteraction>
        where TInteraction : IInteraction
    {
    }
}
