using Domain.Facades.Interfaces;
using Domain.InstructionSets.Interfaces;
using Domain.Interactions;
using Domain.Interactions.DeepScanProspectsForReplies.CheckMessagesHistory;
using Domain.Interactions.DeepScanProspectsForReplies.ClearMessagingSearchCriteria;
using Domain.Interactions.DeepScanProspectsForReplies.EnterSearchMessageCriteria;
using Domain.Interactions.DeepScanProspectsForReplies.GetProspectsMessageItem;
using Domain.Models.DeepScanProspectsForReplies;
using Domain.Models.Networking;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using System.Collections.Generic;

namespace Domain.InstructionSets
{
    public class DeepScanInstructionSet : IDeepScanInstructionSet
    {
        public DeepScanInstructionSet(
            ILogger<DeepScanInstructionSet> logger,
            IDeepScanProspectsInteractionFacade interactionsFacade
            )
        {
            _logger = logger;
            _interactionsFacade = interactionsFacade;
        }

        private readonly ILogger<DeepScanInstructionSet> _logger;
        private readonly IDeepScanProspectsInteractionFacade _interactionsFacade;

        public IList<ProspectRepliedModel> Prospects { get; private set; } = new List<ProspectRepliedModel>();

        public int VisibleConversationCount => _interactionsFacade.ConversationCount;

        public bool ClearMessagingSearchCriteriaInteraction(IWebDriver webDriver)
        {
            ClearMessagingSearchCrtieriaInteraction clearInteraction = new()
            {
                WebDriver = webDriver,
            };

            return _interactionsFacade.HandleClearMessagingCriteriaInteraction(clearInteraction);
        }

        public void BeginDeepScanning(IWebDriver webDriver, IList<NetworkProspectModel> prospects, int visibleMessagesCount)
        {
            foreach (NetworkProspectModel networkProspect in prospects)
            {
                if (ClearMessagingSearchCriteriaInteraction(webDriver) == false)
                {
                    _logger.LogError("ClearMessagingSearchCriteriaInteraction failed");
                    continue;
                }

                if (EnterSearchTerm(webDriver, networkProspect.Name) == false)
                {
                    _logger.LogDebug("EnterSearchTerm failed. Clearing the current search term and moving on.");
                    ClearMessagingSearchCriteriaInteraction(webDriver);
                }

                if (LookForProspectMessages(webDriver, networkProspect.Name, visibleMessagesCount) == false)
                {
                    _logger.LogDebug("No messages found for {0}. Moving onto the next search term", networkProspect.Name);
                    ClearMessagingSearchCriteriaInteraction(webDriver);
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

        private bool EnterSearchTerm(IWebDriver webDriver, string searchCriteria)
        {
            EnterSearchMessageCriteriaInteraction interaction = new()
            {
                SearchCriteria = searchCriteria,
                WebDriver = webDriver
            };

            return _interactionsFacade.HandleEnterSearchmessageCriteriaInteraction(interaction);
        }

        private bool CheckMessageHistoryForRepliesToOurLastMessage(IWebElement messageListItem, NetworkProspectModel networkProspect, IWebDriver webDriver)
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

        public bool GetVisibleConversationCountInteraction(IWebDriver webDriver)
        {
            return GetVisibleConversationCountInteraction(webDriver);
        }

        private bool GetVisibleConversationCount(IWebDriver webDriver)
        {
            InteractionBase interaction = new()
            {
                WebDriver = webDriver
            };

            return _interactionsFacade.HandleGetVisibleConversationsCountInteraction(interaction);
        }
    }
}
