using Domain.Executors.ScanProspectsForReplies.Events;
using Domain.Facades.Interfaces;
using Domain.Interactions;
using Domain.Interactions.ScanProspectsForReplies.GetMessageContent;
using Domain.Interactions.ScanProspectsForReplies.GetNewMessages;
using Domain.Interactions.Shared.CloseAllConversations;
using Domain.Models.RabbitMQMessages;
using Domain.Models.ScanProspectsForReplies;
using Domain.Orchestrators.Interfaces;
using Domain.Providers.Interfaces;
using Domain.Services.Interfaces;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Responses;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;

namespace Domain.Orchestrators
{
    public class ScanProspectsForRepliesPhaseOrchestrator : PhaseOrchestratorBase, IScanProspectsForRepliesPhaseOrchestrator
    {
        public ScanProspectsForRepliesPhaseOrchestrator(
            ILogger<ScanProspectsForRepliesPhaseOrchestrator> logger,
            ITimestampService timestampService,
            IScanProspectsForRepliesInteractionFacade interactionsFacade,
            IWebDriverProvider webDriverProvider)
            : base(logger)
        {
            _interactionsFacade = interactionsFacade;
            _logger = logger;
            _timestampService = timestampService;
            _webDriverProvider = webDriverProvider;
        }

        private readonly IScanProspectsForRepliesInteractionFacade _interactionsFacade;
        private readonly ITimestampService _timestampService;
        private readonly IWebDriverProvider _webDriverProvider;
        private readonly ILogger<ScanProspectsForRepliesPhaseOrchestrator> _logger;
        private IList<NewMessage> NewMessages { get; set; } = new List<NewMessage>();
        public static bool IsRunning { get; set; } = false;

        public event EndOfWorkDayReachedEventHandler EndOfWorkDayReached;
        public event NewMessagesReceivedEventHandler NewMessagesReceived;

        public void Execute(ScanProspectsForRepliesBody message)
        {
            string halId = message.HalId;
            _logger.LogInformation("Executing ScanForProspectRepliesPhase on hal id {halId}", halId);

            IWebDriver webDriver = _webDriverProvider.GetOrCreateWebDriver(BrowserPurpose.ScanForReplies, message.ChromeProfileName, message.GridNamespaceName, message.GridServiceDiscoveryName, out bool isNewWebDriver);
            if (webDriver == null)
            {
                _logger.LogError("WebDriver could not be found or created. Cannot proceed");
                return;
            }

            bool succeeded = GoToPage(webDriver, message.PageUrl);
            if (succeeded == false)
            {
                return;
            }

            ExecuteInternal(message, webDriver);
        }

        private void ExecuteInternal(ScanProspectsForRepliesBody message, IWebDriver webDriver)
        {
            try
            {
                IsRunning = true;
                BeginScanning(message, webDriver);
            }
            finally
            {
                _webDriverProvider.CloseBrowser<IOperationResponse>(BrowserPurpose.ScanForReplies);
                IsRunning = false;
                this.EndOfWorkDayReached.Invoke(this, new EndOfWorkDayReachedEventArgs(message));
                // in case an error occurs during the scan we want to make sure we send any responses to the server
                OutputMessageResponses(message);
            }
        }

        private void BeginScanning(ScanProspectsForRepliesBody message, IWebDriver webDriver)
        {
            DateTimeOffset endOfWorkDayLocal = _timestampService.ParseDateTimeOffsetLocalized(message.TimeZoneId, message.EndOfWorkday);
            while (_timestampService.GetNowLocalized(message.TimeZoneId) < endOfWorkDayLocal)
            {
                bool getNewMessagesSucceeded = GetNewMessages(webDriver);
                if (getNewMessagesSucceeded == false)
                {
                    continue;
                }

                bool closeAllConversationsSucceeded = CloseAllConversations(webDriver);
                if (closeAllConversationsSucceeded == false)
                {
                    _logger.LogDebug("An error occured when attempting to close all of the active conversations on the screen. It is ok, just logging and continuing on.");
                }

                IList<IWebElement> newMessagesElement = _interactionsFacade.NewMessageElements;
                foreach (IWebElement newMessageElement in newMessagesElement)
                {
                    bool getMessageContentSucceeded = GetMessageContent(webDriver, newMessageElement);
                    if (getMessageContentSucceeded == false)
                    {
                        continue;
                    }

                    NewMessage newMessage = _interactionsFacade.NewMessage;
                    if (newMessage != null)
                    {
                        NewMessages.Add(newMessage);
                    }
                }

                OutputMessageResponses(message);
            }
        }

        private bool GetNewMessages(IWebDriver webDriver)
        {
            InteractionBase interaction = new GetNewMessagesInteraction
            {
                WebDriver = webDriver
            };

            return _interactionsFacade.HandleGetNewMessagesInteraction(interaction);
        }

        private bool GetMessageContent(IWebDriver webDriver, IWebElement message)
        {
            InteractionBase interaction = new GetMessageContentInteraction
            {
                WebDriver = webDriver,
                Message = message
            };

            return _interactionsFacade.HandleGetMessageContentInteraction(interaction);
        }

        private bool CloseAllConversations(IWebDriver webDriver)
        {
            InteractionBase interaction = new CloseAllConversationsInteraction
            {
                WebDriver = webDriver
            };

            return _interactionsFacade.HandleCloseConversationsInteraction(interaction);
        }

        private void OutputMessageResponses(ScanProspectsForRepliesBody message)
        {
            if (NewMessages.Count > 0)
            {
                this.NewMessagesReceived.Invoke(this, new NewMessagesReceivedEventArgs(message, NewMessages));
            }
        }
    }
}
