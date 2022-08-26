namespace Domain.Interactions.Networking.SearchResultsLimit.Interfaces
{
    public interface ISearchResultsLimitInteractionHandler<TInteraction> : IInteractionHandler<TInteraction>
        where TInteraction : IInteraction
    {
    }
}
