using Domain.Interactions.DeepScanProspectsForReplies.GetAllVisibleConversationCount.Interfaces;
using Domain.Services.Interfaces.POMs;
using Microsoft.Extensions.Logging;

namespace Domain.Interactions.DeepScanProspectsForReplies.GetAllVisibleConversationCount
{
    public class GetAllVisibleConversationCountInteractionHandler : IGetAllVisibleConversationCountInteractionHandler
    {
        public GetAllVisibleConversationCountInteractionHandler(
            ILogger<GetAllVisibleConversationCountInteractionHandler> logger,
            IDeepScanProspectsServicePOM service)
        {
            _logger = logger;
            _service = service;
        }

        private readonly ILogger<GetAllVisibleConversationCountInteractionHandler> _logger;
        private readonly IDeepScanProspectsServicePOM _service;

        private int ConversationCount { get; set; }

        public bool HandleInteraction(InteractionBase interaction)
        {
            int? visibleMessageCount = _service.GetVisibleConversationCount(interaction.WebDriver);

            if (visibleMessageCount == null)
            {
                return false;
            }

            ConversationCount = (int)visibleMessageCount;

            return true;
        }

        public int GetConversationCount()
        {
            int conversationCount = ConversationCount;
            ConversationCount = 0;
            return conversationCount;
        }
    }
}
