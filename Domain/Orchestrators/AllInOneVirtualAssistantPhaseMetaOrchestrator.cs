using Domain.Executors.AllInOneVirtualAssistant.Events;
using Domain.Executors.MonitorForNewConnections.Events;
using Domain.Executors.ScanProspectsForReplies.Events;
using Domain.Facades.Interfaces;
using Domain.Models.FollowUpMessage;
using Domain.Models.Networking;
using Domain.Models.ProspectList;
using Domain.Models.SendConnections;
using Domain.MQ.Messages;
using Domain.Orchestrators.Interfaces;
using Domain.Providers.Interfaces;
using Leadsly.Application.Model;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;

namespace Domain.Orchestrators
{
    public class AllInOneVirtualAssistantPhaseMetaOrchestrator : PhaseOrchestratorBase, IAllInOneVirtualAssistantPhaseMetaOrchestrator
    {
        private const string PRIMARY_PAGE_URL = "https://www.linkedin.com/mynetwork/invite-connect/connections/";

        public AllInOneVirtualAssistantPhaseMetaOrchestrator(
            ILogger<AllInOneVirtualAssistantPhaseMetaOrchestrator> logger,
            IAllInOneOrchestratorsFacade orchestratorsFacade,
            IWebDriverProvider webDriverProvider) : base(logger)
        {
            _orchestratorsFacade = orchestratorsFacade;
            _logger = logger;
            _webDriverProvider = webDriverProvider;
        }

        private readonly IAllInOneOrchestratorsFacade _orchestratorsFacade;
        private readonly ILogger<AllInOneVirtualAssistantPhaseMetaOrchestrator> _logger;
        private readonly IWebDriverProvider _webDriverProvider;

        #region Properties

        public List<PersistPrimaryProspectModel> PersistPrimaryProspects => _orchestratorsFacade.PersistPrimaryProspects;
        public IList<ConnectionSentModel> ConnectionsSent => _orchestratorsFacade.ConnectionsSent;
        public bool MonthlySearchLimitReached => _orchestratorsFacade.MonthlySearchLimitReached;
        public IList<SentFollowUpMessageModel> SentFollowUpMessages => _orchestratorsFacade.SentFollowUpMessages;
        public IList<SearchUrlProgressModel> UpdatedSearchUrlsProgress => _orchestratorsFacade.UpdatedSearchUrlsProgress;

        #endregion

        #region Events

        public event NewMessagesReceivedEventHandler NewMessagesReceived
        {
            add => _orchestratorsFacade.NewMessagesReceived += value;
            remove => _orchestratorsFacade.NewMessagesReceived -= value;
        }
        public event NewRecentlyAddedProspectsDetectedEventHandler NewConnectionsDetected
        {
            add => _orchestratorsFacade.NewConnectionsDetected += value;
            remove => _orchestratorsFacade.NewConnectionsDetected -= value;
        }

        public event UpdateRecentlyAddedProspectsEventHandler UpdateRecentlyAddedProspects
        {
            add => _orchestratorsFacade.UpdateRecentlyAddedProspects += value;
            remove => _orchestratorsFacade.UpdateRecentlyAddedProspects -= value;
        }

        public event OffHoursNewConnectionsEventHandler OffHoursNewConnectionsDetected
        {
            add => _orchestratorsFacade.OffHoursNewConnectionsDetected += value;
            remove => _orchestratorsFacade.OffHoursNewConnectionsDetected -= value;
        }

        public event ProspectsThatRepliedEventHandler ProspectsThatRepliedDetected
        {
            add => _orchestratorsFacade.ProspectsThatRepliedDetected += value;
            remove => _orchestratorsFacade.ProspectsThatRepliedDetected -= value;
        }

        public event FollowUpMessagesSentEventHandler FollowUpMessagesSent
        {
            add => _orchestratorsFacade.FollowUpMessagesSent += value;
            remove => _orchestratorsFacade.FollowUpMessagesSent -= value;
        }

        #endregion

        public void Execute(AllInOneVirtualAssistantMessageBody message)
        {
            string halId = message.HalId;
            string messageTypeName = nameof(AllInOneVirtualAssistantMessageBody);

            _logger.LogInformation("Executing {0} on HalId {1}", messageTypeName, halId);

            IWebDriver webDriver = _webDriverProvider.GetOrCreateWebDriver(BrowserPurpose.AllInOne, message);
            if (webDriver == null)
            {
                _logger.LogError("Execution of {0} failed. WebDriver could not be found or created. Cannot proceed. HalId: {1}", messageTypeName, message.HalId);
                return;
            }

            if (GoToPage(webDriver, PRIMARY_PAGE_URL))
            {
                _logger.LogError("Failed to navigate to {0}. This phase will exist and nothing else will be executed.", PRIMARY_PAGE_URL);
                return;
            }

            PrimaryWindowHandle = webDriver.CurrentWindowHandle;

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
            //if (message.DeepScanProspectsForReplies != null)
            //{
            // 1. deep scan prospects for replies
            // _orchestratorsFacade.HandleDeepScanProspectsForReplies(webDriver, message.DeepScanProspectsForReplies);

            //// before the follow up message is sent out lets make sure that deepscanprospectsfor replies did not find the prospect in our inbox and one that has replied already
            //IEnumerable<FollowUpMessageBody> followUpMessages = message.FollowUpMessages.Where(f => _orchestratorsFacade.ProspectsThatReplied.Any(x => x.Name == f.ProspectName) == false);
            //message.FollowUpMessages = new Queue<FollowUpMessageBody>(followUpMessages);
            // }

            if (message.CheckOffHoursNewConnections != null)
            {
                // 1. check off hours connections
                _orchestratorsFacade.HandleCheckOffHoursNewConnections(webDriver, message.CheckOffHoursNewConnections);
            }

            // 2. start with monitor for new connections
            _orchestratorsFacade.HandleMonitorForNewConnections(webDriver, message);

            // 3. then execute scan prospects for replies
            _orchestratorsFacade.HandleScanProspectsForReplies(webDriver, message);

            // 4. run follow up messages
            _orchestratorsFacade.HandleFollowUpMessages(webDriver, message);

            // 5. run networking 
            _orchestratorsFacade.HandleNetworking(webDriver, message);

        }

        //private void HandleNetworking(IWebDriver webDriver, AllInOneVirtualAssistantMessageBody message)
        //{
        //    // for each networking message in the queue process it
        //    int length = NetworkingMessages.Count;
        //    for (int i = 0; i < length; i++)
        //    {
        //        NetworkingMessageBody networkingMessage = NetworkingMessages.Dequeue();

        //        string currentWindowHandle = webDriver.CurrentWindowHandle;
        //        try
        //        {
        //            _orchestratorsFacade.HandleNetworking(webDriver, networkingMessage);
        //        }
        //        finally
        //        {
        //            _webDriverProvider.CloseCurrentTab(webDriver, currentWindowHandle);
        //            NumberOfConnectionsSent = 0;
        //        }
        //    }
        //}

        //#region MonitorForNewConnections

        //private void HandleMonitorForNewConnections(IWebDriver webDriver, AllInOneVirtualAssistantMessageBody message)
        //{
        //    ConnectedNetworkProspectsResponse previousMonitoredResponse = message.PreviousMonitoredResponse;
        //    bool getConnectionsCountSucceeded = GetConnectionsCount(webDriver);

        //    if (getConnectionsCountSucceeded == false)
        //    {
        //        _logger.LogDebug("Executing {0}. Failed to determine total connections count. HalId {1}", nameof(AllInOneVirtualAssistantMessageBody), message.HalId);
        //    }

        //    bool getAllRecentlyAddedSucceeded = GetAllRecentlyAddedInteraction(webDriver);

        //    if (getAllRecentlyAddedSucceeded == false)
        //    {
        //        _logger.LogDebug("Executing {0}. Failed to gather all recently added prospects. HalId {1}", nameof(AllInOneVirtualAssistantMessageBody), message.HalId);
        //    }

        //    if (getConnectionsCountSucceeded == true && getAllRecentlyAddedSucceeded == true)
        //    {
        //        // ensure response Items is not null
        //        if (previousMonitoredResponse.Items != null)
        //        {
        //            _logger.LogTrace("Executing {0}. Server has successfully set Items property on {1} response. HalId {2}", nameof(AllInOneVirtualAssistantMessageBody), nameof(ConnectedNetworkProspectsResponse), message.HalId);
        //            int previousConnectionsCount = 0;
        //            IList<RecentlyAddedProspectModel> previousRecentlyAddedProspects = default;

        //            // set previous count and previous recently added prospects to whats on the page currently
        //            if (previousMonitoredResponse.Items.Count == 0)
        //            {
        //                _logger.LogDebug("Executing {0}. {1} response did not contain any prospects to check against. Setting previous values from what is currently displayed on the page. HalId {2}", nameof(AllInOneVirtualAssistantMessageBody), nameof(ConnectedNetworkProspectsResponse), message.HalId);
        //                // persist results to server
        //                previousConnectionsCount = _interactionsFacade.ConnectionsCount;
        //                previousRecentlyAddedProspects = _interactionsFacade.RecentlyAddedProspects;

        //                OutputSaveRecentlyAddedProspects(message, previousConnectionsCount, previousRecentlyAddedProspects);
        //            }
        //            // or set it to whatever the server had saved before
        //            else
        //            {
        //                _logger.LogDebug("Executing {0}. {1} response did contain prospects to check against. HalId {2}", nameof(AllInOneVirtualAssistantMessageBody), nameof(ConnectedNetworkProspectsResponse), message.HalId);
        //                previousConnectionsCount = previousMonitoredResponse.TotalConnectionsCount;
        //                previousRecentlyAddedProspects = previousMonitoredResponse.Items;
        //            }

        //            if (GetAllRecentlyAddedInteraction(webDriver) == true)
        //            {
        //                int currentTotalConnectionsCount = _interactionsFacade.ConnectionsCount;
        //                if (currentTotalConnectionsCount > previousConnectionsCount)
        //                {
        //                    _logger.LogTrace("Executing {0}. Total connections count is greater than previous connection count.", nameof(AllInOneVirtualAssistantMessageBody));
        //                    IList<RecentlyAddedProspectModel> currentRecentlyAddedProspects = _interactionsFacade.RecentlyAddedProspects;

        //                    OutputSaveRecentlyAddedProspects(message, currentTotalConnectionsCount, currentRecentlyAddedProspects);

        //                    IList<RecentlyAddedProspectModel> newRecentlyAddedProspects = currentRecentlyAddedProspects.Where(p => previousRecentlyAddedProspects.Any(prev => prev.Name == p.Name) == false).ToList();

        //                    // invoke an event here
        //                    OutputRecentlyAddedProspects(message, newRecentlyAddedProspects);
        //                }
        //            }
        //        }
        //    }
        //}

        //private bool GetConnectionsCount(IWebDriver webDriver)
        //{
        //    InteractionBase interaction = new GetConnectionsCountInteraction
        //    {
        //        WebDriver = webDriver
        //    };

        //    return _interactionsFacade.HandleGetConnectionsCountInteraction(interaction);
        //}

        //private bool GetAllRecentlyAddedInteraction(IWebDriver webDriver)
        //{
        //    InteractionBase interaction = new GetAllRecentlyAddedInteraction
        //    {
        //        WebDriver = webDriver
        //    };

        //    return _interactionsFacade.HandleGetAllRecentlyAddedInteraction(interaction);
        //}

        //private void OutputRecentlyAddedProspects(AllInOneVirtualAssistantMessageBody message, IList<RecentlyAddedProspectModel> newRecentlyAddedProspects)
        //{
        //    if (newRecentlyAddedProspects != null && newRecentlyAddedProspects.Count > 0)
        //    {
        //        _logger.LogDebug("Executing {0}. Emitting new connections detected event", nameof(AllInOneVirtualAssistantMessageBody));
        //        this.NewConnectionsDetected.Invoke(this, new NewRecentlyAddedProspectsDetectedEventArgs(message, newRecentlyAddedProspects));
        //    }
        //}

        //private void OutputSaveRecentlyAddedProspects(AllInOneVirtualAssistantMessageBody message, int totalConnectionsCount, IList<RecentlyAddedProspectModel> recentlyAddedProspects)
        //{
        //    _logger.LogDebug("Executing {0}. Saving recently added prospects", nameof(AllInOneVirtualAssistantMessageBody));
        //    UpdateRecentlyAddedProspects.Invoke(this, new UpdateRecentlyAddedProspectsEventArgs(message, recentlyAddedProspects, totalConnectionsCount));
        //}

        //#endregion

        //#region ScanProspectsForReplies

        //private void HandleScanProspectsForReplies(IWebDriver webDriver, AllInOneVirtualAssistantMessageBody message)
        //{
        //    if (GetAllUnreadMessageListBubbles(webDriver, message.HalId) == false)
        //    {
        //        _logger.LogWarning("Executing {0}. Failed to get all unread messages from the messages list bubble. HalId {1}", nameof(AllInOneVirtualAssistantMessageBody), message.HalId);
        //    }

        //    if (GetUnreadMessagesContent(webDriver) == false)
        //    {
        //        _logger.LogWarning("Executing {0}. Failed to get unread messages content. HalId {1}", nameof(AllInOneVirtualAssistantMessageBody), message.HalId);
        //    }

        //    OutputMessageResponses(message);
        //}

        //private bool GetAllUnreadMessageListBubbles(IWebDriver webDriver, string halId)
        //{
        //    InteractionBase interaction = new GetAllUnreadMessagesInteraction
        //    {
        //        HalId = halId,
        //        WebDriver = webDriver
        //    };

        //    return _interactionsFacade.HandleGetAllUnreadMessageListBubbles(interaction);
        //}

        //private bool GetUnreadMessagesContent(IWebDriver webDriver)
        //{
        //    IList<IWebElement> unreadMessages = _interactionsFacade.UnreadMessages;
        //    if (unreadMessages.Count > 0)
        //    {
        //        InteractionBase interaction = new GetUnreadMessagesContentInteraction
        //        {
        //            WebDriver = webDriver,
        //            Messages = unreadMessages
        //        };

        //        return _interactionsFacade.HandleGetUnreadMessagesContent(interaction);
        //    }

        //    return true;
        //}

        //private void OutputMessageResponses(AllInOneVirtualAssistantMessageBody message)
        //{
        //    IList<NewMessageModel> newMessages = _interactionsFacade.NewMessages;
        //    if (newMessages.Count > 0)
        //    {
        //        this.NewMessagesReceived.Invoke(this, new NewMessagesReceivedEventArgs(message, newMessages));
        //    }
        //}

        //// 1. Check for any new message notifications (the blue number next to prospect)
        //// if any exist process as expected

        //// run the below steps only once
        //// I DONT REMEMBER WHY WE NEED TO FETCH LAST SCANNED PROSPECTS FROM THE SERVER?
        //// 2. set all messages list bubbles from either the server or current page

        //// 3. grab all messages list bubbles from the current page

        //// 4. check if any new prospects appear in the list

        //// 5. if they do process as expected

        //// 6. just scan the page for new notification icons and pop up messages

        //// 7. on new pop up message, extract details and process as expected, then close the pop up

        //// 8. on new notification icon, open the conversation and repeat step 7

        //#endregion

        //#region FollowUpMessages

        //private void HandleFollowUpMessages(IWebDriver webDriver, AllInOneVirtualAssistantMessageBody message)
        //{
        //    int length = FollowUpMessages.Count;
        //    try
        //    {
        //        if (FollowUpMessages.Any() == true)
        //        {
        //            string pageUrl = FollowUpMessages.Peek().PageUrl;
        //            if (PrepareBrowserWindow(webDriver, pageUrl) == false)
        //            {
        //                return;
        //            }
        //        }

        //        for (int i = 0; i < length; i++)
        //        {
        //            FollowUpMessageBody followUpMessage = FollowUpMessages.Dequeue();

        //            // before the follow up message is sent out lets make sure that deepscanprospectsfor replies did not find the prospect in our inbox and one that has replied already
        //            if (ShouldSend(followUpMessage) == true)
        //            {
        //                SendFollowUpMessage(webDriver, followUpMessage);
        //            }
        //        }
        //    }
        //    finally
        //    {
        //        OutputFollowUpMessagesSent(message);

        //        SwitchBackToMainTab(webDriver);
        //    }
        //}

        //private bool ShouldSend(FollowUpMessageBody followUpMessage)
        //{
        //    IList<ProspectRepliedModel> prospectsThatReplied = _instructionsSetFacade.ProspectsThatReplied;
        //    if (prospectsThatReplied.Count > 0)
        //    {
        //        if (prospectsThatReplied.Any(p => p.Name == followUpMessage.ProspectName))
        //        {
        //            return false;
        //        }
        //    }

        //    return true;
        //}

        //private void SendFollowUpMessage(IWebDriver webDriver, FollowUpMessageBody message)
        //{
        //    try
        //    {
        //        _instructionsSetFacade.SendFollowUpMessage(webDriver, message);

        //        SentFollowUpMessageModel sentFollowUpMessage = _instructionsSetFacade.GetSentFollowUpMessage();
        //        if (sentFollowUpMessage != null)
        //        {
        //            SentFollowUpMessages.Add(sentFollowUpMessage);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Unexpected exception occured while executing {0} phase", nameof(FollowUpMessageBody));
        //    }
        //}

        //private void OutputFollowUpMessagesSent(AllInOneVirtualAssistantMessageBody message)
        //{
        //    if (SentFollowUpMessages != null && SentFollowUpMessages.Count > 0)
        //    {
        //        this.FollowUpMessagesSent.Invoke(this, new FollowUpMessagesSentEventArgs(message, SentFollowUpMessages));
        //    }
        //}

        //#endregion

        // #region Networking



        //private void BeginNetworking(NetworkingMessageBody message, IWebDriver webDriver)
        //{
        //    _logger.LogDebug("Begning to execute networking phase");
        //    foreach (SearchUrlProgressModel searchUrlProgress in message.SearchUrlsProgress)
        //    {
        //        try
        //        {
        //            if (PrepareBrowser(webDriver, searchUrlProgress, message.HalId) == false)
        //            {
        //                return;
        //            }

        //            if (_instructionsSetFacade.GetTotalnumberOfSearchResultsInteraction(webDriver, searchUrlProgress) == false)
        //            {
        //                // just move onto the next search url
        //                _logger.LogDebug($"Unable to determine total number of search results. Moving on to the next search url in the list. This failure occured for search result: {searchUrlProgress.SearchUrl}");
        //                continue;
        //            }

        //            int totalResults = _interactionsFacade.TotalNumberOfSearchResults;
        //            _instructionsSetFacade.Add_UpdateSearchUrlProgressRequest(searchUrlProgress.SearchUrlProgressId, searchUrlProgress.LastPage, webDriver.Url, totalResults, webDriver.CurrentWindowHandle);

        //            _instructionsSetFacade.ConnectWithProspectsForSearchUrl(webDriver, message, searchUrlProgress, (int)totalResults);

        //            // see if we can get away with this.
        //            if (NumberOfConnectionsSent >= message.ProspectsToCrawl)
        //                break;

        //            if (MonthlySearchLimitReached == true)
        //                break;
        //        }
        //        finally
        //        {
        //            SwitchBackToMainTab(webDriver);
        //        }
        //    }
        //    _logger.LogDebug("Finished executing networking phase");
        //}

        //private bool PrepareBrowser(IWebDriver webDriver, SearchUrlProgressModel searchUrlProgress, string halId)
        //{
        //    string messageTypeName = nameof(NetworkingMessageBody);
        //    if (PrepareBrowserWindow(webDriver, searchUrlProgress.SearchUrl) == false)
        //    {
        //        _logger.LogError("Execution of {0} failed. Failed to switch to an existing tab or create a new one. Hal id {1}", messageTypeName, halId);
        //        return false;
        //    }

        //    if (_instructionsSetFacade.NoSearchResultsDisplayedInteraction(webDriver) == true)
        //    {
        //        _logger.LogError("Execution of {0} failed. No search results page displayed again. This means we've tried refreshing and waiting for results page to be displayed, but it wasn't.", messageTypeName);
        //        return false;
        //    }

        //    return true;
        //}

        //public bool GetMonthlySearchLimitReached()
        //{
        //    return _instructionsSetFacade.GetMonthlySearchLimitReached();
        //}

        //public IList<SearchUrlProgressModel> GetUpdatedSearchUrlsProgress()
        //{
        //    return _instructionsSetFacade.GetUpdatedSearchUrls();
        //}

        //#endregion

        //#region DeepScanProspectsForReplies

        //private void HandleDeepScanProspectsForReplies(IWebDriver webDriver, AllInOneVirtualAssistantMessageBody message)
        //{
        //    if (message.DeepScanProspectsForReplies != null)
        //    {
        //        if (PrepareBrowserWindow(webDriver, message.DeepScanProspectsForReplies.PageUrl) == false)
        //        {
        //            return;
        //        }

        //        int visibleMessagesCount = 0;
        //        if (_instructionsSetFacade.GetVisibleConversationCountInteraction(webDriver) == true)
        //        {
        //            visibleMessagesCount = _instructionsSetFacade.VisibleConversationCount;
        //        }

        //        if (visibleMessagesCount == 0)
        //        {
        //            _logger.LogDebug("There are no messages in this user's inbox. No need to run {0} phase", nameof(DeepScanProspectsForRepliesBody));
        //            return;
        //        }

        //        try
        //        {
        //            _instructionsSetFacade.BeginDeepScanning(webDriver, message.DeepScanProspectsForReplies.NetworkProspects, visibleMessagesCount);
        //        }
        //        finally
        //        {
        //            _instructionsSetFacade.ClearMessagingSearchCriteriaInteraction(webDriver);
        //            OutputProspectsThatReplied(message);

        //            SwitchBackToMainTab(webDriver);
        //        }
        //    }
        //}

        //private void OutputProspectsThatReplied(AllInOneVirtualAssistantMessageBody message)
        //{
        //    IList<ProspectRepliedModel> prospectsThatReplied = _instructionsSetFacade.ProspectsThatReplied;
        //    if (prospectsThatReplied != null && prospectsThatReplied.Count > 0)
        //    {
        //        _logger.LogDebug("{0} found prospects that replied!", nameof(DeepScanProspectsForRepliesBody));
        //        this.ProspectsThatRepliedDetected.Invoke(this, new ProspectsThatRepliedEventArgs(message, prospectsThatReplied));
        //    }
        //}

        //#endregion

        //#region CheckOffHoursNewConnections

        //private void HandleCheckOffHoursNewConnections(IWebDriver webDriver, AllInOneVirtualAssistantMessageBody message)
        //{
        //    if (message.CheckOffHoursNewConnections != null)
        //    {
        //        try
        //        {
        //            _instructionsSetFacade.BeginCheckingForNewConnectionsFromOffHours(webDriver, message.CheckOffHoursNewConnections);
        //        }
        //        finally
        //        {
        //            OutputCheckOffHoursNewConnections(message);
        //        }
        //    }
        //}

        //private void OutputCheckOffHoursNewConnections(AllInOneVirtualAssistantMessageBody message)
        //{
        //    IList<RecentlyAddedProspectModel> recentlyAddedProspects = _instructionsSetFacade.RecentlyAddedProspects;
        //    if (recentlyAddedProspects != null && recentlyAddedProspects.Count > 0)
        //    {
        //        _logger.LogDebug("{0} found new connections!", nameof(CheckOffHoursNewConnectionsBody));
        //        this.OffHoursNewConnectionsDetected.Invoke(this, new OffHoursNewConnectionsEventArgs(message, recentlyAddedProspects));
        //    }
        //}

        //#endregion

        //private bool PrepareBrowserWindow(IWebDriver webDriver, string pageUrl)
        //{
        //    if (SwitchToNewTab(webDriver) == false)
        //    {
        //        return false;
        //    }

        //    if (GoToPage(webDriver, pageUrl) == false)
        //    {
        //        return false;
        //    }

        //    return true;
        //}
    }
}
