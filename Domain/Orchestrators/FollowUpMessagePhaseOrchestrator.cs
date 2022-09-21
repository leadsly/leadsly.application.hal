using Domain.Facades.Interfaces;
using Domain.Interactions;
using Domain.Interactions.FollowUpMessage.CreateNewMessage;
using Domain.Interactions.FollowUpMessage.EnterMessage;
using Domain.Interactions.FollowUpMessage.EnterProspectName;
using Domain.Models.FollowUpMessage;
using Domain.MQ.Messages;
using Domain.Orchestrators.Interfaces;
using Domain.Providers.Interfaces;
using Leadsly.Application.Model;
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
            IWebDriverProvider webDriverProvider)
            : base(logger)
        {
            _interactionFacade = interactionFacade;
            _logger = logger;
            _webDriverProvider = webDriverProvider;
        }

        private readonly IFollowUpMessageInteractionFacade _interactionFacade;
        private readonly ILogger<FollowUpMessagePhaseOrchestrator> _logger;
        private readonly IWebDriverProvider _webDriverProvider;
        private SentFollowUpMessageModel SentFollowUpMessage { get; set; }

        public SentFollowUpMessageModel GetSentFollowUpMessage()
        {
            SentFollowUpMessageModel item = SentFollowUpMessage;
            SentFollowUpMessage = null;
            return item;
        }

        public void Execute(FollowUpMessageBody message)
        {
            string halId = message.HalId;
            string messageTypeName = nameof(FollowUpMessageBody);
            _logger.LogInformation("Executing {0} on HalId {1}", messageTypeName, halId);

            IWebDriver webDriver = _webDriverProvider.GetOrCreateWebDriver(BrowserPurpose.FollowUpMessages, message);
            if (webDriver == null)
            {
                _logger.LogError("Execution of {0} failed. WebDriver could not be found or created. Cannot proceed. HalId: {1}", messageTypeName, message.HalId);
                return;
            }

            if (GoToPage(webDriver, message.PageUrl) == false)
            {
                _logger.LogError("Execution of {0} failed. WebDriver could not navigate to the given PageUrl {1}. HalId {2}", messageTypeName, message.PageUrl, message.HalId);
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
                _logger.LogError(ex, "Unexpected exception occured while executing {0} phase", nameof(FollowUpMessageBody));
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

            bool enterMessageSucceeded = EnterMessageInteraction(webDriver, message.Content, message.OrderNum);
            if (enterMessageSucceeded == false)
            {
                return;
            }

            SentFollowUpMessage = _interactionFacade.SentFollowUpMessage;
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

        private bool EnterMessageInteraction(IWebDriver webDriver, string content, int orderNum)
        {
            InteractionBase interaction = new EnterMessageInteraction
            {
                Content = content,
                WebDriver = webDriver,
                OrderNum = orderNum
            };

            return _interactionFacade.HandleEnterMessageInteraction(interaction);
        }
    }
}
