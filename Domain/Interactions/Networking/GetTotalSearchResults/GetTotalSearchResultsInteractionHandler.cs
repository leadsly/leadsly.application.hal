using Domain.Interactions.Networking.GetTotalSearchResults.Interfaces;
using Domain.Services.Interfaces.POMs;
using Microsoft.Extensions.Logging;

namespace Domain.Interactions.Networking.GetTotalSearchResults
{
    public class GetTotalSearchResultsInteractionHandler : IGetTotalSearchResultsInteractionHandler
    {
        public GetTotalSearchResultsInteractionHandler(
            ILogger<GetTotalSearchResultsInteractionHandler> logger,
            ISearchPageFooterServicePOM service)
        {
            _logger = logger;
            _service = service;
        }

        private readonly ILogger<GetTotalSearchResultsInteractionHandler> _logger;
        private readonly ISearchPageFooterServicePOM _service;
        private int TotalResults { get; set; }

        public bool HandleInteraction(InteractionBase interaction)
        {
            GetTotalSearchResultsInteraction getTotalInteraction = interaction as GetTotalSearchResultsInteraction;

            if (getTotalInteraction.TotalNumberOfResults == 0)
            {
                int? totalNumberOfResults = _service.GetTotalResults(getTotalInteraction.WebDriver, true);
                if (totalNumberOfResults == null)
                {
                    return false;
                }
                TotalResults = (int)totalNumberOfResults;
            }
            else
            {
                TotalResults = getTotalInteraction.TotalNumberOfResults;
            }

            return true;
        }

        public int GetTotalResults()
        {
            int totalResults = TotalResults;
            TotalResults = 0;
            return totalResults;
        }
    }
}
