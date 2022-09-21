using Domain.Executors.MonitorForNewConnections.Events;
using Domain.Executors.ScanProspectsForReplies.Events;
using Domain.Facades.Interfaces;
using Domain.InstructionSets.Interfaces;
using Domain.Interactions;
using Domain.Interactions.AllInOneVirtualAssistant.GetAllUnreadMessages;
using Domain.Interactions.AllInOneVirtualAssistant.GetMessageContent;
using Domain.Interactions.FollowUpMessage.CreateNewMessage;
using Domain.Interactions.FollowUpMessage.EnterMessage;
using Domain.Interactions.FollowUpMessage.EnterProspectName;
using Domain.Interactions.MonitorForNewConnections.GetAllRecentlyAdded;
using Domain.Interactions.MonitorForNewConnections.GetConnectionsCount;
using Domain.Interactions.Networking.GetTotalSearchResults;
using Domain.Interactions.Networking.NoResultsFound;
using Domain.Models.AllInOneVirtualAssistant;
using Domain.Models.FollowUpMessage;
using Domain.Models.MonitorForNewProspects;
using Domain.Models.Networking;
using Domain.Models.ProspectList;
using Domain.Models.Responses;
using Domain.Models.ScanProspectsForReplies;
using Domain.Models.SendConnections;
using Domain.MQ.Messages;
using Domain.Orchestrators.Interfaces;
using Domain.Providers.Interfaces;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Responses;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Domain.Orchestrators
{
    public class AllInOneVirtualAssistantPhaseOrchestrator : PhaseOrchestratorBase, IAllInOneVirtualAssistantPhaseOrchestrator
    {
        public AllInOneVirtualAssistantPhaseOrchestrator(
            ILogger<AllInOneVirtualAssistantPhaseOrchestrator> logger,
            IAllInOneVirtualAssistantInteractionFacade interactionsFacade,
            IConnectWithProspectsForSearchUrlInstructionSet instructionSet,
            IWebDriverProvider webDriverProvider) : base(logger)
        {
            _instructionSet = instructionSet;
            _interactionsFacade = interactionsFacade;
            _logger = logger;
            _webDriverProvider = webDriverProvider;
        }

        public event NewMessagesReceivedEventHandler NewMessagesReceived;
        public PreviouslyConnectedNetworkProspectsModel PreviouslyConnectedNetworkProspects { get; private set; } = new();
        public event NewRecentlyAddedProspectsDetectedEventHandler NewConnectionsDetected;
        private readonly IConnectWithProspectsForSearchUrlInstructionSet _instructionSet;
        private readonly IAllInOneVirtualAssistantInteractionFacade _interactionsFacade;
        private readonly ILogger<AllInOneVirtualAssistantPhaseOrchestrator> _logger;
        private readonly IWebDriverProvider _webDriverProvider;
        private IList<SentFollowUpMessageModel> SentFollowUpMessages { get; set; } = new List<SentFollowUpMessageModel>();
        public IList<SentFollowUpMessageModel> GetSentFollowUpMessages()
        {
            IList<SentFollowUpMessageModel> items = SentFollowUpMessages;
            SentFollowUpMessages = null;
            return items;
        }
        private IList<SearchUrlProgressModel> UpdatedSearchUrlsProgress => _instructionSet.UpdatedSearchUrlsProgress;
        private bool MonthlySearchLimitReached => _instructionSet.MonthlySearchLimitReached;
        private int NumberOfConnectionsSent
        {
            get
            {
                return _instructionSet.NumberOfConnectionsSent;
            }
            set
            {
                _instructionSet.NumberOfConnectionsSent = value;
            }
        }
        private Queue<FollowUpMessageBody> FollowUpMessages { get; set; }
        private Queue<NetworkingMessageBody> NetworkingMessages { get; set; }

        public List<PersistPrimaryProspectModel> PersistPrimaryProspects => _instructionSet.GetPersistPrimaryProspects();

        public IList<ConnectionSentModel> ConnectionsSent => _instructionSet.GetConnectionsSent();

        public IList<RecentlyAddedProspectModel> RecentlyAddedProspects { get; private set; }

        public int PreviousTotalConnectionsCount { get; private set; }

        public void Execute(AllInOneVirtualAssistantMessageBody message)
        {
            string halId = message.HalId;
            string messageTypeName = nameof(AllInOneVirtualAssistantMessageBody);
            FollowUpMessages = message.FollowUpMessages;
            NetworkingMessages = message.NetworkingMessages;

            _logger.LogInformation("Executing {0} on HalId {1}", messageTypeName, halId);

            IWebDriver webDriver = _webDriverProvider.GetOrCreateWebDriver(BrowserPurpose.AllInOne, message);
            if (webDriver == null)
            {
                _logger.LogError("Execution of {0} failed. WebDriver could not be found or created. Cannot proceed. HalId: {1}", messageTypeName, message.HalId);
                return;
            }

            ExecuteInternal(webDriver, message);
        }

        private void ExecuteInternal(IWebDriver webDriver, AllInOneVirtualAssistantMessageBody message)
        {
            try
            {
                BeginVirtualAssistantWork(webDriver, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected exception occured while executing {0} phase", nameof(AllInOneVirtualAssistantMessageBody));
            }
        }

        private void BeginVirtualAssistantWork(IWebDriver webDriver, AllInOneVirtualAssistantMessageBody message)
        {
            // 1. start with monitor for new connections
            HandleMonitorForNewConnections(webDriver, message);

            // 2. then execute scan prospects for replies
            HandleScanProspectsForReplies(webDriver, message);

            // 3. run follow up messages
            HandleFollowUpMessages(webDriver, message);

            // 4. run networking 
            HandleNetworking(webDriver, message);

            // 5. post message to the server we're done, start deprovisioning process

        }

        #region MonitorForNewConnections

        private void HandleMonitorForNewConnections(IWebDriver webDriver, AllInOneVirtualAssistantMessageBody message)
        {
            PreviouslyConnectedNetworkProspectsResponse previousMonitoredResponse = message.PreviousMonitoredResponse;
            bool getConnectionsCountSucceeded = GetConnectionsCount(webDriver);

            if (getConnectionsCountSucceeded == false)
            {
                _logger.LogDebug("Executing {0}. Failed to determine total connections count. HalId {1}", nameof(AllInOneVirtualAssistantMessageBody), message.HalId);
            }

            bool getAllRecentlyAddedSucceeded = GetAllRecentlyAddedInteraction(webDriver);

            if (getAllRecentlyAddedSucceeded == false)
            {
                _logger.LogDebug("Executing {0}. Failed to gather all recently added prospects. HalId {1}", nameof(AllInOneVirtualAssistantMessageBody), message.HalId);
            }

            if (getConnectionsCountSucceeded == true && getAllRecentlyAddedSucceeded == true)
            {
                // ensure response Items is not null
                if (previousMonitoredResponse.Items != null)
                {
                    _logger.LogTrace("Executing {0}. Server has successfully set Items property on {1} response. HalId {2}", nameof(AllInOneVirtualAssistantMessageBody), nameof(PreviouslyConnectedNetworkProspectsResponse), message.HalId);
                    int previousConnectionsCount = 0;
                    IList<RecentlyAddedProspectModel> previousRecentlyAddedProspects = default;

                    // set previous count and previous recently added prospects to whats on the page currently
                    if (previousMonitoredResponse.Items.Count == 0)
                    {
                        _logger.LogDebug("Executing {0}. {1} response did not contain any prospects to check against. Setting previous values from what is currently displayed on the page. HalId {2}", nameof(AllInOneVirtualAssistantMessageBody), nameof(PreviouslyConnectedNetworkProspectsResponse), message.HalId);
                        // persist results to server
                        previousConnectionsCount = _interactionsFacade.ConnectionsCount;
                        previousRecentlyAddedProspects = _interactionsFacade.RecentlyAddedProspects;
                    }
                    // or set it to whatever the server had saved before
                    else
                    {
                        _logger.LogDebug("Executing {0}. {1} response did contain prospects to check against. HalId {2}", nameof(AllInOneVirtualAssistantMessageBody), nameof(PreviouslyConnectedNetworkProspectsResponse), message.HalId);
                        previousConnectionsCount = previousMonitoredResponse.PreviousTotalConnectionsCount;
                        previousRecentlyAddedProspects = previousMonitoredResponse.Items;
                    }

                    if (GetAllRecentlyAddedInteraction(webDriver) == true)
                    {
                        int currentTotalConnectionsCount = _interactionsFacade.ConnectionsCount;
                        if (currentTotalConnectionsCount > previousConnectionsCount)
                        {
                            _logger.LogTrace("Executing {0}. Total connections count is greater than previous connection count.", nameof(AllInOneVirtualAssistantMessageBody));
                            IList<RecentlyAddedProspectModel> currentRecentlyAddedProspects = _interactionsFacade.RecentlyAddedProspects;

                            PreviouslyConnectedNetworkProspects.PreviousTotalConnectionsCount = currentTotalConnectionsCount;
                            PreviouslyConnectedNetworkProspects.Items = currentRecentlyAddedProspects;

                            IList<RecentlyAddedProspectModel> newRecentlyAddedProspects = currentRecentlyAddedProspects.Where(p => previousRecentlyAddedProspects.Any(prev => prev.Name == p.Name) == false).ToList();

                            // invoke an event here
                            OutputRecentlyAddedProspects(message, newRecentlyAddedProspects);
                        }
                    }
                }
            }
        }

        private bool GetConnectionsCount(IWebDriver webDriver)
        {
            InteractionBase interaction = new GetConnectionsCountInteraction
            {
                WebDriver = webDriver
            };

            return _interactionsFacade.HandleGetConnectionsCountInteraction(interaction);
        }

        private bool GetAllRecentlyAddedInteraction(IWebDriver webDriver)
        {
            InteractionBase interaction = new GetAllRecentlyAddedInteraction
            {
                WebDriver = webDriver
            };

            return _interactionsFacade.HandleGetAllRecentlyAddedInteraction(interaction);
        }

        private void OutputRecentlyAddedProspects(AllInOneVirtualAssistantMessageBody message, IList<RecentlyAddedProspectModel> newRecentlyAddedProspects)
        {
            if (newRecentlyAddedProspects != null && newRecentlyAddedProspects.Count > 0)
            {
                _logger.LogDebug("Executing {0}. Emitting new connections detected event", nameof(AllInOneVirtualAssistantMessageBody));
                this.NewConnectionsDetected.Invoke(this, new NewRecentlyAddedProspectsDetectedEventArgs(message, newRecentlyAddedProspects));
            }
        }

        public PreviouslyConnectedNetworkProspectsModel GetPreviouslyConnectedNetworkProspects()
        {
            PreviouslyConnectedNetworkProspectsModel model = PreviouslyConnectedNetworkProspects;
            PreviouslyConnectedNetworkProspects = new();
            return model;
        }

        #endregion

        #region ScanProspectsForReplies

        private void HandleScanProspectsForReplies(IWebDriver webDriver, AllInOneVirtualAssistantMessageBody message)
        {
            if (GetAllUnreadMessageListBubbles(webDriver, message.HalId) == false)
            {
                _logger.LogWarning("Executing {0}. Failed to get all unread messages from the messages list bubble. HalId {1}", nameof(AllInOneVirtualAssistantMessageBody), message.HalId);
            }

            if (GetUnreadMessagesContent(webDriver) == false)
            {
                _logger.LogWarning("Executing {0}. Failed to get unread messages content. HalId {1}", nameof(AllInOneVirtualAssistantMessageBody), message.HalId);
            }

            OutputMessageResponses(message);
        }

        private bool GetAllUnreadMessageListBubbles(IWebDriver webDriver, string halId)
        {
            InteractionBase interaction = new GetAllUnreadMessagesInteraction
            {
                HalId = halId,
                WebDriver = webDriver
            };

            return _interactionsFacade.HandleGetAllUnreadMessageListBubbles(interaction);
        }

        private bool GetUnreadMessagesContent(IWebDriver webDriver)
        {
            IList<IWebElement> unreadMessages = _interactionsFacade.UnreadMessages;
            if (unreadMessages.Count > 0)
            {
                InteractionBase interaction = new GetMessagesContentInteraction
                {
                    WebDriver = webDriver,
                    Messages = unreadMessages
                };

                return _interactionsFacade.HandleGetUnreadMessagesContent(interaction);
            }

            return true;
        }

        private void OutputMessageResponses(AllInOneVirtualAssistantMessageBody message)
        {
            IList<NewMessageModel> newMessages = _interactionsFacade.NewMessages;
            if (newMessages.Count > 0)
            {
                this.NewMessagesReceived.Invoke(this, new NewMessagesReceivedEventArgs(message, newMessages));
            }
        }

        // 1. Check for any new message notifications (the blue number next to prospect)
        // if any exist process as expected

        // run the below steps only once
        // I DONT REMEMBER WHY WE NEED TO FETCH LAST SCANNED PROSPECTS FROM THE SERVER?
        // 2. set all messages list bubbles from either the server or current page

        // 3. grab all messages list bubbles from the current page

        // 4. check if any new prospects appear in the list

        // 5. if they do process as expected

        // 6. just scan the page for new notification icons and pop up messages

        // 7. on new pop up message, extract details and process as expected, then close the pop up

        // 8. on new notification icon, open the conversation and repeat step 7

        #endregion

        #region FollowUpMessages

        private void HandleFollowUpMessages(IWebDriver webDriver, AllInOneVirtualAssistantMessageBody message)
        {
            int length = FollowUpMessages.Count;
            for (int i = 0; i < length; i++)
            {
                FollowUpMessageBody followUpMessage = FollowUpMessages.Dequeue();

                if (CreateNewMessageInteraction(webDriver) == false)
                {
                    continue;
                }

                if (EnterProspectNameInteraction(webDriver, followUpMessage.ProspectName) == false)
                {
                    continue;
                }

                if (EnterMessageInteraction(webDriver, followUpMessage.Content, followUpMessage.OrderNum) == false)
                {
                    continue;
                }

                SentFollowUpMessageModel model = _interactionsFacade.SentFollowUpMessage;
                if (model != null)
                {
                    SentFollowUpMessages.Add(model);
                }
            }
        }

        private bool CreateNewMessageInteraction(IWebDriver webDriver)
        {
            InteractionBase interaction = new CreateNewMessageInteraction
            {
                WebDriver = webDriver
            };

            return _interactionsFacade.HandleCreateNewMessageInteraction(interaction);
        }

        private bool EnterProspectNameInteraction(IWebDriver webDriver, string prospectName)
        {
            InteractionBase interaction = new EnterProspectNameInteraction
            {
                ProspectName = prospectName,
                WebDriver = webDriver
            };

            return _interactionsFacade.HandleEnterProspectNameInteraction(interaction);
        }

        private bool EnterMessageInteraction(IWebDriver webDriver, string content, int orderNum)
        {
            InteractionBase interaction = new EnterMessageInteraction
            {
                Content = content,
                WebDriver = webDriver,
                OrderNum = orderNum
            };

            return _interactionsFacade.HandleEnterMessageInteraction(interaction);
        }

        #endregion

        #region Networking

        public bool GetMonthlySearchLimitReached()
        {
            return _instructionSet.GetMonthlySearchLimitReached();
        }

        public IList<SearchUrlProgressModel> GetUpdatedSearchUrlsProgress()
        {
            return _instructionSet.GetUpdatedSearchUrls();
        }

        private void HandleNetworking(IWebDriver webDriver, AllInOneVirtualAssistantMessageBody message)
        {
            // for each networking message in the queue process it
            int length = NetworkingMessages.Count;
            for (int i = 0; i < length; i++)
            {
                NetworkingMessageBody networkingMessage = NetworkingMessages.Dequeue();

                string currentWindowHandle = webDriver.CurrentWindowHandle;
                try
                {
                    BeginNetworking(networkingMessage, webDriver);
                }
                finally
                {
                    _webDriverProvider.CloseCurrentTab(webDriver, currentWindowHandle);
                    NumberOfConnectionsSent = 0;
                }
            }
        }

        private void BeginNetworking(NetworkingMessageBody message, IWebDriver webDriver)
        {
            _logger.LogDebug("Begning to execute networking phase");
            foreach (SearchUrlProgressModel searchUrlProgress in message.SearchUrlsProgress)
            {
                if (PrepareBrowser(webDriver, searchUrlProgress, message.HalId) == false)
                {
                    return;
                }

                if (GetTotalnumberOfSearchResults(webDriver, searchUrlProgress) == false)
                {
                    // just move onto the next search url
                    _logger.LogDebug($"Unable to determine total number of search results. Moving on to the next search url in the list. This failure occured for search result: {searchUrlProgress.SearchUrl}");
                    continue;
                }

                int totalResults = _interactionsFacade.TotalNumberOfSearchResults;
                Add_UpdateSearchUrlProgressRequest(searchUrlProgress.SearchUrlProgressId, searchUrlProgress.LastPage, webDriver.Url, totalResults, webDriver.CurrentWindowHandle);

                _instructionSet.ConnectWithProspectsForSearchUrl(webDriver, message, searchUrlProgress, (int)totalResults);

                // see if we can get away with this.
                if (NumberOfConnectionsSent >= message.ProspectsToCrawl)
                    break;

                if (MonthlySearchLimitReached == true)
                    break;
            }
            _logger.LogDebug("Finished executing networking phase");
        }

        private void Add_UpdateSearchUrlProgressRequest(string searchUrlProgressId, int currentPage, string currentUrl, int totalResults, string currentWindowHandle)
        {
            if (UpdatedSearchUrlsProgress.Any(req => req.SearchUrlProgressId == searchUrlProgressId) == false)
            {
                UpdatedSearchUrlsProgress.Add(new()
                {
                    SearchUrlProgressId = searchUrlProgressId,
                    StartedCrawling = true,
                    LastPage = currentPage,
                    SearchUrl = currentUrl,
                    TotalSearchResults = totalResults,
                    WindowHandleId = currentWindowHandle
                });
            }
        }

        private bool GetTotalnumberOfSearchResults(IWebDriver webDriver, SearchUrlProgressModel searchUrlProgress)
        {
            InteractionBase interaction = new GetTotalSearchResultsInteraction
            {
                WebDriver = webDriver,
                TotalNumberOfResults = searchUrlProgress.TotalSearchResults
            };

            return _interactionsFacade.HandleGetTotalNumberOfSearchResults(interaction);
        }

        private bool PrepareBrowser(IWebDriver webDriver, SearchUrlProgressModel searchUrlProgress, string halId)
        {
            string messageTypeName = nameof(NetworkingMessageBody);
            HalOperationResult<IOperationResponse> result = _webDriverProvider.SwitchToOrNewTab<IOperationResponse>(webDriver, searchUrlProgress.WindowHandleId);
            if (result.Succeeded == false)
            {
                _logger.LogError("Execution of {0} failed. Failed to switch to an existing tab or create a new one. Hal id {1}", messageTypeName, halId);
                return false;
            }

            bool pageNavigationSucceeded = GoToPage(webDriver, searchUrlProgress.SearchUrl);
            if (pageNavigationSucceeded == false)
            {
                _logger.LogError("Execution of {0} failed. Failed to navigate to page {1}. Hal id {2}", messageTypeName, searchUrlProgress.SearchUrl, halId);
                return false;
            }

            if (NoSearchResultsDisplayed(webDriver) == true)
            {
                _logger.LogError("Execution of {0} failed. No search results page displayed again. This means we've tried refreshing and waiting for results page to be displayed, but it wasn't.", messageTypeName);
                return false;
            }

            return true;
        }

        private bool NoSearchResultsDisplayed(IWebDriver webDriver)
        {
            NoResultsFoundInteraction interaction = new()
            {
                WebDriver = webDriver
            };

            return _interactionsFacade.HandleNoResultsFoundInteraction(interaction);
        }

        #endregion
    }
}
