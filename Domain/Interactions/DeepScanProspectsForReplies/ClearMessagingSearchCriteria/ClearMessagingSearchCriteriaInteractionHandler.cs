using Domain.Interactions.DeepScanProspectsForReplies.ClearMessagingSearchCriteria.Interfaces;
using Domain.Services.Interfaces.POMs;
using Microsoft.Extensions.Logging;

namespace Domain.Interactions.DeepScanProspectsForReplies.ClearMessagingSearchCriteria
{
    public class ClearMessagingSearchCriteriaInteractionHandler : IClearMessagingSearchCriteriaInteractionHandler
    {
        public ClearMessagingSearchCriteriaInteractionHandler(
            ILogger<ClearMessagingSearchCriteriaInteractionHandler> logger,
            IDeepScanProspectsServicePOM service)
        {
            _logger = logger;
            _service = service;
        }

        private readonly IDeepScanProspectsServicePOM _service;
        private readonly ILogger<ClearMessagingSearchCriteriaInteractionHandler> _logger;
        public bool HandleInteraction(InteractionBase interaction)
        {
            ClearMessagingSearchCrtieriaInteraction clearMessagingSearchCrtieriaInteraction = interaction as ClearMessagingSearchCrtieriaInteraction;
            bool succeeded = _service.ClearMessagingSearchCriteria(clearMessagingSearchCrtieriaInteraction.WebDriver);
            if (succeeded == false)
            {
                // we can try to retry here
            }

            return succeeded;
        }
    }
}
