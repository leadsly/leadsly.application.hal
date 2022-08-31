using Domain.Interactions.DeepScanProspectsForReplies.GetProspectsMessageItem.Interfaces;
using Domain.Services.Interfaces.POMs;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;

namespace Domain.Interactions.DeepScanProspectsForReplies.GetProspectsMessageItem
{
    public class GetProspectsMessageItemInteractionHandler : IGetProspectsMessageItemInteractionHandler<GetProspectsMessageItemInteraction>
    {
        public GetProspectsMessageItemInteractionHandler(
            ILogger<GetProspectsMessageItemInteractionHandler> logger,
            IDeepScanProspectsService service)
        {
            _logger = logger;
            _service = service;
        }

        private readonly IDeepScanProspectsService _service;
        private readonly ILogger<GetProspectsMessageItemInteractionHandler> _logger;

        public bool HandleInteraction(GetProspectsMessageItemInteraction interaction)
        {
            IWebElement prospectMessageItem = _service.GetProspectsMessageItem(interaction.WebDriver, interaction.ProspectName, interaction.MessagesCountBefore);
            if (prospectMessageItem == null)
            {
                _logger.LogDebug("Failed to locate prospect's message item");
                return false;
            }
            // click on the new message
            bool clickSucceeded = _service.ClickNewMessage(prospectMessageItem, interaction.WebDriver);
            if (clickSucceeded == false)
            {
                _logger.LogDebug("Failed to click on the new message");
                return false;
            }

            return true;
        }
    }
}
