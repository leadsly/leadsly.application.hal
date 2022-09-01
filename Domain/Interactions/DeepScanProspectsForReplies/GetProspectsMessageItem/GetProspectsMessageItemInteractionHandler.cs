using Domain.Interactions.DeepScanProspectsForReplies.GetProspectsMessageItem.Interfaces;
using Domain.Services.Interfaces.POMs;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using System.Collections.Generic;

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

        private IList<IWebElement> ProspectMessageItems { get; set; } = new List<IWebElement>();

        public bool HandleInteraction(GetProspectsMessageItemInteraction interaction)
        {
            // its possible to have multiple results for a search criteria so we want to scan all results
            IList<IWebElement> prospectMessageItems = _service.GetProspectsMessageItems(interaction.WebDriver, interaction.ProspectName, interaction.MessagesCountBefore);
            if (prospectMessageItems == null)
            {
                _logger.LogDebug("Failed to locate prospect's message item");
                return false;
            }

            ProspectMessageItems = prospectMessageItems;

            return true;
        }

        public IList<IWebElement> GetProspectMessageItems()
        {
            IList<IWebElement> messageItems = ProspectMessageItems;
            ProspectMessageItems = new List<IWebElement>();
            return messageItems;
        }
    }
}
