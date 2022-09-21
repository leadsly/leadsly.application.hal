namespace Domain.Interactions.Networking.GetTotalSearchResults.Interfaces
{
    public interface IGetTotalSearchResultsInteractionHandler : IInteractionHandler
    {
        public int GetTotalResults();
    }
}
