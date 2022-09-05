﻿using Domain.Facades.Interfaces;
using Domain.Interactions;
using Domain.Interactions.FollowUpMessage.CreateNewMessage;
using Domain.Interactions.FollowUpMessage.EnterMessage;
using Domain.Interactions.FollowUpMessage.EnterProspectName;
using Domain.Models.Requests;
using Domain.Orchestrators.Interfaces;
using Domain.Providers.Interfaces;
using Domain.Services.Interfaces;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.WebDriver;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using System;

namespace Domain.Orchestrators
{
    public class FollowUpMessagePhaseOrchestrator : PhaseOrchestratorBase, IFollowUpMessagePhaseOrchestrator
    {
        public FollowUpMessagePhaseOrchestrator(
            ILogger<FollowUpMessagePhaseOrchestrator> logger,
            IFollowUpMessageInteractionFacade interactionFacade,
            ITimestampService timestampService,
            IWebDriverProvider webDriverProvider)
            : base(logger)
        {
            _interactionFacade = interactionFacade;
            _logger = logger;
            _webDriverProvider = webDriverProvider;
            _timestampService = timestampService;
        }

        private readonly IFollowUpMessageInteractionFacade _interactionFacade;
        private readonly ILogger<FollowUpMessagePhaseOrchestrator> _logger;
        private readonly IWebDriverProvider _webDriverProvider;
        private readonly ITimestampService _timestampService;
        private SentFollowUpMessageRequest SentFollowUpMessageRequest { get; set; }

        public SentFollowUpMessageRequest GetSentFollowUpMessage()
        {
            SentFollowUpMessageRequest request = SentFollowUpMessageRequest;
            SentFollowUpMessageRequest = null;
            return request;
        }

        public void Execute(FollowUpMessageBody message)
        {
            string halId = message.HalId;
            _logger.LogInformation("Executing FollowUpMessageBody on hal id {halId}", halId);

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

            ExecuteInternal(webDriver, message);
        }

        private void ExecuteInternal(IWebDriver webDriver, FollowUpMessageBody message)
        {
            try
            {
                BeginSendingFollowUpMessage(webDriver, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected exception occured while executing FollowUpMessage phase");
            }
        }

        private void BeginSendingFollowUpMessage(IWebDriver webDriver, FollowUpMessageBody message)
        {
            bool createNewMessageSucceeded = CreateNewMessageInteraction(webDriver);
            if (createNewMessageSucceeded == false)
            {
                return;
            }

            bool enterProspectNameSucceeded = EnterProspectNameInteraction(webDriver, message.ProspectName);
            if (enterProspectNameSucceeded == false)
            {
                return;
            }

            bool enterMessageSucceeded = EnterMessageInteraction(webDriver, message.Content);
            if (enterMessageSucceeded == false)
            {
                return;
            }

            SentFollowUpMessageRequest = new()
            {
                MessageOrderNum = message.OrderNum,
                CampaignProspectId = message.CampaignProspectId,
                ActualDeliveryDateTimeStamp = _timestampService.TimestampNow()
            };
        }

        private bool CreateNewMessageInteraction(IWebDriver webDriver)
        {
            InteractionBase interaction = new CreateNewMessageInteraction
            {
                WebDriver = webDriver
            };

            return _interactionFacade.HandleCreateNewMessageInteraction(interaction);
        }

        private bool EnterProspectNameInteraction(IWebDriver webDriver, string prospectName)
        {
            InteractionBase interaction = new EnterProspectNameInteraction
            {
                ProspectName = prospectName,
                WebDriver = webDriver
            };

            return _interactionFacade.HandleEnterProspectNameInteraction(interaction);
        }

        private bool EnterMessageInteraction(IWebDriver webDriver, string content)
        {
            InteractionBase interaction = new EnterMessageInteraction
            {
                Content = content,
                WebDriver = webDriver
            };

            return _interactionFacade.HandleEnterMessageInteraction(interaction);
        }
    }
}
