using Domain.Interactions.DeepScanProspectsForReplies.EnterSearchMessageCriteria.Interfaces;
using Domain.Services.Interfaces.POMs;
using Microsoft.Extensions.Logging;

namespace Domain.Interactions.DeepScanProspectsForReplies.EnterSearchMessageCriteria
{
    public class EnterSearchMessageCriteriaInteractionHandler : IEnterSearchMessageCriteriaInteractionHandler<EnterSearchMessageCriteriaInteraction>
    {
        public EnterSearchMessageCriteriaInteractionHandler(
            ILogger<EnterSearchMessageCriteriaInteractionHandler> logger,
            IDeepScanProspectsService service)
        {
            _logger = logger;
            _service = service;
        }

        private readonly IDeepScanProspectsService _service;
        private readonly ILogger<EnterSearchMessageCriteriaInteractionHandler> _logger;

        public bool HandleInteraction(EnterSearchMessageCriteriaInteraction interaction)
        {
            bool succeeded = _service.EnterSearchMessagesCriteria(interaction.WebDriver, interaction.SearchCriteria);
            if (succeeded == false)
            {
                // repeat here if we wanted to                
            }

            return succeeded;
        }
    }
}
