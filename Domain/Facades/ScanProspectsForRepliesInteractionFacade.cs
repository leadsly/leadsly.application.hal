using Domain.Facades.Interfaces;
using Domain.Interactions;
using Domain.Interactions.ScanProspectsForReplies.GetMessageContent.Interfaces;
using Domain.Interactions.ScanProspectsForReplies.GetNewMessages.Interfaces;
using Domain.Interactions.Shared.CloseAllConversations.Interfaces;
using Leadsly.Application.Model.Requests;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using System.Collections.Generic;

namespace Domain.Facades
{
    public class ScanProspectsForRepliesInteractionFacade : IScanProspectsForRepliesInteractionFacade
    {
        public ScanProspectsForRepliesInteractionFacade(
            ILogger<ScanProspectsForRepliesInteractionFacade> logger,
            IGetMessageContentInteractionHandler getMessageContentHandler,
            ICloseAllConversationsInteractionHandler closeConversationsHandler,
            IGetNewMessagesInteractionHandler getNewMessagesHandler)
        {
            _closeConversationsHandler = closeConversationsHandler;
            _logger = logger;
            _getMessageContentHandler = getMessageContentHandler;
            _getNewMessagesHandler = getNewMessagesHandler;
        }

        private readonly ICloseAllConversationsInteractionHandler _closeConversationsHandler;
        private readonly ILogger<ScanProspectsForRepliesInteractionFacade> _logger;
        private readonly IGetMessageContentInteractionHandler _getMessageContentHandler;
        private readonly IGetNewMessagesInteractionHandler _getNewMessagesHandler;

        public IList<IWebElement> NewMessages => _getNewMessagesHandler.GetNewMessages();

        public NewMessageRequest NewMessageRequest => _getMessageContentHandler.GetNewMessageRequest();

        public bool HandleGetMessageContentInteraction(InteractionBase interaction)
        {
            return _getMessageContentHandler.HandleInteraction(interaction);
        }

        public bool HandleGetNewMessagesInteraction(InteractionBase interaction)
        {
            return _getNewMessagesHandler.HandleInteraction(interaction);
        }

        public bool HandleCloseConversationsInteraction(InteractionBase interaction)
        {
            return _closeConversationsHandler.HandleInteraction(interaction);
        }
    }
}
