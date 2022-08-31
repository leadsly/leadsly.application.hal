using Domain.Interactions.DeepScanProspectsForReplies.ClearMessagingSearchCriteria.Interfaces;
using Domain.Services.Interfaces.POMs;
using Microsoft.Extensions.Logging;

namespace Domain.Interactions.DeepScanProspectsForReplies.ClearMessagingSearchCriteria
{
    public class ClearMessagingSearchCriteriaInteractionHandler : IClearMessagingSearchCriteriaInteractionHandler<ClearMessagingSearchCrtieriaInteraction>
    {
        public ClearMessagingSearchCriteriaInteractionHandler(
            ILogger<ClearMessagingSearchCriteriaInteractionHandler> logger,
            IDeepScanProspectsService service)
        {
            _logger = logger;
            _service = service;
        }

        private readonly IDeepScanProspectsService _service;
        private readonly ILogger<ClearMessagingSearchCriteriaInteractionHandler> _logger;
        public bool HandleInteraction(ClearMessagingSearchCrtieriaInteraction interaction)
        {
            bool succeeded = _service.ClearMessagingSearchCriteria(interaction.WebDriver);
            if (succeeded == false)
            {
                // we can try to retry here
            }

            return succeeded;
        }
    }
}
