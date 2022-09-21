using Domain.Interactions.AllInOneVirtualAssistant.GetMessageContent.Interfaces;
using Domain.Interactions.Shared.CloseAllConversations;
using Domain.Interactions.Shared.CloseAllConversations.Interfaces;
using Domain.Models.ScanProspectsForReplies;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using System.Collections.Generic;

namespace Domain.Interactions.AllInOneVirtualAssistant.GetMessageContent
{
    public class GetMessagesContentInteractionHandler : IGetMessagesContentInteractionHandler
    {
        public GetMessagesContentInteractionHandler(
            ILogger<GetMessagesContentInteractionHandler> logger,
            ICloseAllConversationsInteractionHandler closeAllConversationsHandler,
            IGetMessageContentInteractionHandler getMessageContentHandler)
        {
            _logger = logger;
            _getMessageContentHandler = getMessageContentHandler;
            _closeAllConversationsHandler = closeAllConversationsHandler;
        }

        private readonly ILogger<GetMessagesContentInteractionHandler> _logger;
        private readonly IGetMessageContentInteractionHandler _getMessageContentHandler;
        private readonly ICloseAllConversationsInteractionHandler _closeAllConversationsHandler;
        private IList<NewMessageModel> NewMessages { get; set; } = new List<NewMessageModel>();

        public bool HandleInteraction(InteractionBase interaction)
        {
            IWebDriver webDriver = interaction.WebDriver;
            GetMessagesContentInteraction getMessagesContentInteraction = interaction as GetMessagesContentInteraction;

            foreach (IWebElement messageListItem in getMessagesContentInteraction.Messages)
            {
                if (GetMessageContent(webDriver, messageListItem) == false)
                {
                    _logger.LogDebug("Could not get message content from the current active message list item. Moving onto the next one");
                    CloseAllConversations(webDriver);
                    continue;
                }

                NewMessageModel newMessage = _getMessageContentHandler.GetNewMessage();
                NewMessages.Add(newMessage);

                CloseAllConversations(webDriver);
            }

            return true;
        }

        private bool GetMessageContent(IWebDriver webDriver, IWebElement messageListItem)
        {
            GetMessageContentInteraction interaction = new()
            {
                Message = messageListItem,
                WebDriver = webDriver
            };

            return _getMessageContentHandler.HandleInteraction(interaction);
        }

        private bool CloseAllConversations(IWebDriver webDriver)
        {
            InteractionBase interaction = new CloseAllConversationsInteraction
            {
                WebDriver = webDriver
            };

            return _closeAllConversationsHandler.HandleInteraction(interaction);
        }

        public IList<NewMessageModel> GetNewMessages()
        {
            IList<NewMessageModel> newMessages = NewMessages;
            NewMessages = new List<NewMessageModel>();
            return newMessages;
        }
    }
}
