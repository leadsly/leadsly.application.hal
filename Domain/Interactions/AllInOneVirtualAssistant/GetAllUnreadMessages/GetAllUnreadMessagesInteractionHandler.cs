using Domain.Interactions.AllInOneVirtualAssistant.GetAllUnreadMessages.Interfaces;
using Domain.Services.Interfaces.POMs;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using System.Collections.Generic;

namespace Domain.Interactions.AllInOneVirtualAssistant.GetAllUnreadMessages
{
    public class GetAllUnreadMessagesInteractionHandler : IGetAllUnreadMessagesInteractionHandler
    {
        private readonly ILogger<GetAllUnreadMessagesInteractionHandler> _logger;
        private readonly IMessageListBubbleServicePOM _service;

        public GetAllUnreadMessagesInteractionHandler(
            ILogger<GetAllUnreadMessagesInteractionHandler> logger,
            IMessageListBubbleServicePOM service
            )
        {
            _logger = logger;
            _service = service;
        }

        private IList<IWebElement> UnreadMessages { get; set; } = new List<IWebElement>();

        public bool HandleInteraction(InteractionBase interaction)
        {
            IWebDriver webDriver = interaction.WebDriver;
            GetAllUnreadMessagesInteraction getUnreadInteraction = interaction as GetAllUnreadMessagesInteraction;
            // 1. get all unread messages
            IList<IWebElement> messagesListBubbles = _service.GetMessagesListBubbles(webDriver);
            if (messagesListBubbles == null)
            {
                _logger.LogInformation("Could not locate any messages list bubbles. There was probably an issue with the webdriver locating them. HalId: {0}", getUnreadInteraction.HalId);
                return false;
            }

            // 2. get all unread messages
            IList<IWebElement> unreadMessagesListBubbles = _service.GetUnreadMessagesListBubbles(messagesListBubbles);
            if (unreadMessagesListBubbles == null)
            {
                _logger.LogWarning("Unread messages list bubble was null. Something went wrong filtering down the list. HalId {0}", getUnreadInteraction.HalId);
                return false;
            }

            if (unreadMessagesListBubbles.Count == 0)
            {
                _logger.LogInformation("There are no unread messages that exist in the messages list bubble. HalId {0}", getUnreadInteraction.HalId);
            }

            UnreadMessages = unreadMessagesListBubbles;

            return true;
        }

        public IList<IWebElement> GetUnreadMessages()
        {
            IList<IWebElement> unreadMessages = UnreadMessages;
            UnreadMessages = new List<IWebElement>();
            return unreadMessages;
        }
    }
}
