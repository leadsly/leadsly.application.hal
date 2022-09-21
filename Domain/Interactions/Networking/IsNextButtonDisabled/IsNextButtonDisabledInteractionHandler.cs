using Domain.Interactions.Networking.IsNextButtonDisabled.Interfaces;
using Domain.Services.Interfaces.POMs;
using Microsoft.Extensions.Logging;

namespace Domain.Interactions.Networking.IsNextButtonDisabled
{
    public class IsNextButtonDisabledInteractionHandler : IIsNextButtonDisabledInteractionHandler
    {
        private readonly ILogger<IsNextButtonDisabledInteractionHandler> _logger;
        private readonly ISearchPageFooterServicePOM _service;

        public IsNextButtonDisabledInteractionHandler(
            ILogger<IsNextButtonDisabledInteractionHandler> logger,
            ISearchPageFooterServicePOM service)
        {
            _logger = logger;
            _service = service;
        }

        public bool HandleInteraction(InteractionBase interaction)
        {
            bool disabled = false;
            bool? isNextButtonClickable = _service.IsNextButtonClickable(interaction.WebDriver);
            if (isNextButtonClickable == null)
            {
                _logger.LogInformation("Next button is null. Could not locate it. Checking if it is the last page");
                disabled = true;
            }
            else if (isNextButtonClickable == false)
            {
                _logger.LogInformation("Next button is disabled. Checking if it is the last page");
                disabled = true;
            }
            else
            {
                _logger.LogDebug("Next button is not disabled");
                disabled = false;
            }

            return disabled;
        }
    }
}
