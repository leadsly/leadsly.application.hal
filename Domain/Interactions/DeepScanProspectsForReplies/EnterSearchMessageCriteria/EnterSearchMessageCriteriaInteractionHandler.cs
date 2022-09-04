using Domain.Interactions.DeepScanProspectsForReplies.EnterSearchMessageCriteria.Interfaces;
using Domain.Services.Interfaces.POMs;
using Microsoft.Extensions.Logging;

namespace Domain.Interactions.DeepScanProspectsForReplies.EnterSearchMessageCriteria
{
    public class EnterSearchMessageCriteriaInteractionHandler : IEnterSearchMessageCriteriaInteractionHandler
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

        public bool HandleInteraction(InteractionBase interaction)
        {
            EnterSearchMessageCriteriaInteraction enterSearchMessageCriteriaInteraction = interaction as EnterSearchMessageCriteriaInteraction;
            bool succeeded = _service.EnterSearchMessagesCriteria(enterSearchMessageCriteriaInteraction.WebDriver, enterSearchMessageCriteriaInteraction.SearchCriteria);
            if (succeeded == false)
            {
                // repeat here if we wanted to                
            }

            return succeeded;
        }
    }
}
