using Domain.Executors.ScanProspectsForReplies.Events;
using Domain.Interactions.ScanProspectsForReplies.ScanProspects;
using Domain.Interactions.ScanProspectsForReplies.ScanProspects.Interfaces;
using Domain.Orchestrators.Interfaces;
using Domain.Providers.Campaigns;
using Domain.Providers.Interfaces;
using Domain.Services.Interfaces;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.Requests;
using Leadsly.Application.Model.Responses;
using Leadsly.Application.Model.WebDriver;
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
            IScanProspectsInteractionHandler<ScanProspectsInteraction> interactionHandler,
            IWebDriverProvider webDriverProvider)
            : base(logger)
        {
            _interactionHandler = interactionHandler;
            _logger = logger;
            _timestampService = timestampService;
            _webDriverProvider = webDriverProvider;
        }

        private readonly IScanProspectsInteractionHandler<ScanProspectsInteraction> _interactionHandler;
        private readonly ITimestampService _timestampService;
        private readonly IWebDriverProvider _webDriverProvider;
        private readonly ILogger<ScanProspectsForRepliesPhaseOrchestrator> _logger;

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
            ScanProspectsInteraction interaction = new()
            {
                WebDriver = webDriver
            };

            DateTimeOffset endOfWorkDayLocal = _timestampService.ParseDateTimeOffsetLocalized(message.TimeZoneId, message.EndOfWorkday);
            while (_timestampService.GetNowLocalized(message.TimeZoneId) < endOfWorkDayLocal)
            {
                _interactionHandler.HandleInteraction(interaction);

                OutputMessageResponses(message);

                webDriver.Navigate().Refresh();
            }
        }

        private void OutputMessageResponses(ScanProspectsForRepliesBody message)
        {
            IList<NewMessageRequest> newMessageRequests = _interactionHandler.GetNewMessageRequests();
            if (newMessageRequests.Count > 0)
            {
                this.NewMessagesReceived.Invoke(this, new NewMessagesReceivedEventArgs(message, newMessageRequests));
            }
        }
    }
}
