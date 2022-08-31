namespace Domain.Interactions.DeepScanProspectsForReplies.EnterSearchMessageCriteria.Interfaces
{
    public interface IEnterSearchMessageCriteriaInteractionHandler<TInteraction> : IInteractionHandler<TInteraction>
        where TInteraction : IInteraction
    {
    }
}
