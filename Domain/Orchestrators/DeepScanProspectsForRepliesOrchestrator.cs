using Domain.Facades.Interfaces;
using Domain.Interactions.DeepScanProspectsForReplies.CheckMessagesHistory;
using Domain.Interactions.DeepScanProspectsForReplies.ClearMessagingSearchCriteria;
using Domain.Interactions.DeepScanProspectsForReplies.EnterSearchMessageCriteria;
using Domain.Interactions.DeepScanProspectsForReplies.GetProspectsMessageItem;
using Domain.Models;
using Domain.Models.Responses;
using Domain.Orchestrators.Interfaces;
using Domain.Providers.Interfaces;
using Domain.Services.Interfaces.POMs;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.WebDriver;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using System.Collections.Generic;

namespace Domain.Orchestrators
{
    public class DeepScanProspectsForRepliesOrchestrator : PhaseOrchestratorBase, IDeepScanProspectsForRepliesPhaseOrchestrator
    {
        public DeepScanProspectsForRepliesOrchestrator(
            ILogger<DeepScanProspectsForRepliesOrchestrator> logger,
            IDeepScanProspectsInteractionFacade interactionsFacade,
            IDeepScanProspectsServicePOM pomService,
            IWebDriverProvider webDriverProvider)
            : base(logger)
        {
            _logger = logger;
            _webDriverProvider = webDriverProvider;
            _pomService = pomService;
            _interactionsFacade = interactionsFacade;
        }

        private readonly IDeepScanProspectsServicePOM _pomService;
        private readonly IDeepScanProspectsInteractionFacade _interactionsFacade;
        private readonly IWebDriverProvider _webDriverProvider;
        private readonly ILogger<DeepScanProspectsForRepliesOrchestrator> _logger;

        public IList<ProspectReplied> Prospects { get; set; } = new List<ProspectReplied>();

        public void Execute(DeepScanProspectsForRepliesBody message, IList<NetworkProspectResponse> prospects)
        {
            string halId = message.HalId;
            _logger.LogInformation("Executing DeepScanProspectsForRepliesBody on hal id {halId}", halId);

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

            int visibleMessagesCount = _pomService.GetVisibleConversationCount(webDriver);
            if (visibleMessagesCount == 0)
            {
                _logger.LogDebug("There are no messages in this user's inbox. No need to run DeepScanProspectsForReplies phase");
                return;
            }

            ExecuteInternal(message, webDriver, prospects, visibleMessagesCount);
        }

        private void ExecuteInternal(DeepScanProspectsForRepliesBody message, IWebDriver webDriver, IList<NetworkProspectResponse> prospects, int visibleMessagesCount)
        {
            try
            {
                BeginDeepScanning(message, webDriver, prospects, visibleMessagesCount);
            }
            finally
            {
                ClearMessagingSearchCriteria(webDriver);
            }
        }

        private void BeginDeepScanning(DeepScanProspectsForRepliesBody message, IWebDriver webDriver, IList<NetworkProspectResponse> prospects, int visibleMessagesCount)
        {
            foreach (NetworkProspectResponse networkProspect in prospects)
            {
                if (ClearMessagingSearchCriteria(webDriver) == false)
                {
                    _logger.LogError("ClearMessagingSearchCriteriaInteraction failed");
                    continue;
                }

                if (EnterSearchTerm(webDriver, networkProspect.Name) == false)
                {
                    _logger.LogDebug("EnterSearchTerm failed. Clearing the current search term and moving on.");
                    ClearMessagingSearchCriteria(webDriver);
                }

                if (LookForProspectMessages(webDriver, networkProspect.Name, visibleMessagesCount) == false)
                {
                    _logger.LogDebug("No messages found for {0}. Moving onto the next search term", networkProspect.Name);
                    ClearMessagingSearchCriteria(webDriver);
                    continue;
                }

                foreach (IWebElement messageListItem in _interactionsFacade.ProspectMessageListItems)
                {
                    if (CheckMessageHistoryForRepliesToOurLastMessage(messageListItem, networkProspect, webDriver) == false)
                    {
                        _logger.LogError("CheckMessagesHistoryInteraction failed");
                        continue;
                    }

                    Prospects.Add(_interactionsFacade.ProspectReplied);
                }
            }
        }

        private bool ClearMessagingSearchCriteria(IWebDriver webDriver)
        {
            ClearMessagingSearchCrtieriaInteraction clearInteraction = new()
            {
                WebDriver = webDriver,
            };

            return _interactionsFacade.HandleClearMessagingCriteriaInteraction(clearInteraction);
        }


        private bool EnterSearchTerm(IWebDriver webDriver, string searchCriteria)
        {
            EnterSearchMessageCriteriaInteraction interaction = new()
            {
                SearchCriteria = searchCriteria,
                WebDriver = webDriver
            };

            return _interactionsFacade.HandleEnterSearchmessageCriteriaInteraction(interaction);
        }

        private bool CheckMessageHistoryForRepliesToOurLastMessage(IWebElement messageListItem, NetworkProspectResponse networkProspect, IWebDriver webDriver)
        {
            CheckMessagesHistoryInteraction checkMessageContents = new()
            {
                CampaignProspectId = networkProspect.CampaignProspectId,
                ProspectName = networkProspect.Name,
                TargetMessage = networkProspect.LastFollowUpMessageContent,
                WebDriver = webDriver,
                MessageListItem = messageListItem
            };

            return _interactionsFacade.HandleCheckMessagesHistoryInteraction(checkMessageContents);
        }

        private bool LookForProspectMessages(IWebDriver webDriver, string prospectName, int messagesCountBefore)
        {
            GetProspectsMessageItemInteraction interaction = new()
            {
                MessagesCountBefore = messagesCountBefore,
                ProspectName = prospectName,
                WebDriver = webDriver
            };

            return _interactionsFacade.HandleGetProspectsMessageItemInteraction(interaction);
        }
    }
}
