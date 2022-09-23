using Domain.InstructionSets.Interfaces;
using Domain.Models.DeepScanProspectsForReplies;
using Domain.Models.Networking;
using Domain.MQ.Messages;
using Domain.Orchestrators.Interfaces;
using Domain.Providers.Interfaces;
using Leadsly.Application.Model;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using System.Collections.Generic;

namespace Domain.Orchestrators
{
    public class DeepScanProspectsForRepliesOrchestrator : PhaseOrchestratorBase, IDeepScanProspectsForRepliesPhaseOrchestrator
    {
        public DeepScanProspectsForRepliesOrchestrator(
            ILogger<DeepScanProspectsForRepliesOrchestrator> logger,
            IDeepScanInstructionSet instructionsSet,
            IWebDriverProvider webDriverProvider)
            : base(logger)
        {
            _logger = logger;
            _webDriverProvider = webDriverProvider;
            _instructionsSet = instructionsSet;
        }

        private readonly IDeepScanInstructionSet _instructionsSet;
        private readonly IWebDriverProvider _webDriverProvider;
        private readonly ILogger<DeepScanProspectsForRepliesOrchestrator> _logger;

        public IList<ProspectRepliedModel> Prospects => _instructionsSet.Prospects;

        public void Execute(DeepScanProspectsForRepliesBody message, IList<NetworkProspectModel> prospects)
        {
            string halId = message.HalId;
            string messageTypeName = nameof(DeepScanProspectsForRepliesBody);
            _logger.LogInformation("Executing {0} on HalId {1}", messageTypeName, halId);

            IWebDriver webDriver = _webDriverProvider.GetOrCreateWebDriver(BrowserPurpose.ScanForReplies, message);
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

            int visibleMessagesCount = 0;
            if (_instructionsSet.GetVisibleConversationCountInteraction(webDriver) == true)
            {
                visibleMessagesCount = _instructionsSet.VisibleConversationCount;
            }

            if (visibleMessagesCount == 0)
            {
                _logger.LogDebug("There are no messages in this user's inbox. No need to run DeepScanProspectsForReplies phase");
                return;
            }

            ExecuteInternal(webDriver, prospects, visibleMessagesCount);
        }

        private void ExecuteInternal(IWebDriver webDriver, IList<NetworkProspectModel> prospects, int visibleMessagesCount)
        {
            try
            {
                _instructionsSet.BeginDeepScanning(webDriver, prospects, visibleMessagesCount);
            }
            finally
            {
                _instructionsSet.ClearMessagingSearchCriteriaInteraction(webDriver);
            }
        }

        //private void BeginDeepScanning(IWebDriver webDriver, IList<NetworkProspectModel> prospects, int visibleMessagesCount)
        //{
        //    foreach (NetworkProspectModel networkProspect in prospects)
        //    {
        //        if (ClearMessagingSearchCriteria(webDriver) == false)
        //        {
        //            _logger.LogError("ClearMessagingSearchCriteriaInteraction failed");
        //            continue;
        //        }

        //        if (EnterSearchTerm(webDriver, networkProspect.Name) == false)
        //        {
        //            _logger.LogDebug("EnterSearchTerm failed. Clearing the current search term and moving on.");
        //            ClearMessagingSearchCriteria(webDriver);
        //        }

        //        if (LookForProspectMessages(webDriver, networkProspect.Name, visibleMessagesCount) == false)
        //        {
        //            _logger.LogDebug("No messages found for {0}. Moving onto the next search term", networkProspect.Name);
        //            ClearMessagingSearchCriteria(webDriver);
        //            continue;
        //        }

        //        foreach (IWebElement messageListItem in _interactionsFacade.ProspectMessageListItems)
        //        {
        //            if (CheckMessageHistoryForRepliesToOurLastMessage(messageListItem, networkProspect, webDriver) == false)
        //            {
        //                _logger.LogError("CheckMessagesHistoryInteraction failed");
        //                continue;
        //            }

        //            Prospects.Add(_interactionsFacade.ProspectReplied);
        //        }
        //    }
        //}

        //private bool ClearMessagingSearchCriteria(IWebDriver webDriver)
        //{
        //    ClearMessagingSearchCrtieriaInteraction clearInteraction = new()
        //    {
        //        WebDriver = webDriver,
        //    };

        //    return _interactionsFacade.HandleClearMessagingCriteriaInteraction(clearInteraction);
        //}


        //private bool EnterSearchTerm(IWebDriver webDriver, string searchCriteria)
        //{
        //    EnterSearchMessageCriteriaInteraction interaction = new()
        //    {
        //        SearchCriteria = searchCriteria,
        //        WebDriver = webDriver
        //    };

        //    return _interactionsFacade.HandleEnterSearchmessageCriteriaInteraction(interaction);
        //}

        //private bool CheckMessageHistoryForRepliesToOurLastMessage(IWebElement messageListItem, NetworkProspectModel networkProspect, IWebDriver webDriver)
        //{
        //    CheckMessagesHistoryInteraction checkMessageContents = new()
        //    {
        //        CampaignProspectId = networkProspect.CampaignProspectId,
        //        ProspectName = networkProspect.Name,
        //        TargetMessage = networkProspect.LastFollowUpMessageContent,
        //        WebDriver = webDriver,
        //        MessageListItem = messageListItem
        //    };

        //    return _interactionsFacade.HandleCheckMessagesHistoryInteraction(checkMessageContents);
        //}

        //private bool LookForProspectMessages(IWebDriver webDriver, string prospectName, int messagesCountBefore)
        //{
        //    GetProspectsMessageItemInteraction interaction = new()
        //    {
        //        MessagesCountBefore = messagesCountBefore,
        //        ProspectName = prospectName,
        //        WebDriver = webDriver
        //    };

        //    return _interactionsFacade.HandleGetProspectsMessageItemInteraction(interaction);
        //}
    }
}
