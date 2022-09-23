using Domain.Facades.Interfaces;
using Domain.InstructionSets.Interfaces;
using Domain.Models.DeepScanProspectsForReplies;
using Domain.Models.FollowUpMessage;
using Domain.Models.MonitorForNewProspects;
using Domain.Models.Networking;
using Domain.Models.ProspectList;
using Domain.Models.SendConnections;
using Domain.MQ.Messages;
using OpenQA.Selenium;
using System.Collections.Generic;

namespace Domain.Facades
{
    public class AllInOneInstructionsSetFacade : IAllInOneInstructionsSetFacade
    {
        public AllInOneInstructionsSetFacade(
            IFollowUpMessageInstructionSet followUpInstructionSet,
            INetworkingInstructionSet networkingInstructionSet,
            IDeepScanInstructionSet deepScanInstructionSet,
            ICheckForNewConnectionsFromOffHoursInstructionSet checkOffHoursInstructionSet
            )
        {
            _followUpInstructionSet = followUpInstructionSet;
            _checkOffHoursInstructionSet = checkOffHoursInstructionSet;
            _networkingInstructionSet = networkingInstructionSet;
            _deepScanInstructionSet = deepScanInstructionSet;
        }

        private readonly IFollowUpMessageInstructionSet _followUpInstructionSet;
        private readonly ICheckForNewConnectionsFromOffHoursInstructionSet _checkOffHoursInstructionSet;
        private readonly INetworkingInstructionSet _networkingInstructionSet;
        private readonly IDeepScanInstructionSet _deepScanInstructionSet;

        public int NumberOfConnectionsSent
        {
            get => _networkingInstructionSet.NumberOfConnectionsSent;
            set => _networkingInstructionSet.NumberOfConnectionsSent = value;
        }

        public bool MonthlySearchLimitReached => _networkingInstructionSet.MonthlySearchLimitReached;
        public IList<SearchUrlProgressModel> UpdatedSearchUrlsProgress => _networkingInstructionSet.UpdatedSearchUrlsProgress;
        public IList<ProspectRepliedModel> ProspectsThatReplied => _deepScanInstructionSet.Prospects;
        public int VisibleConversationCount => _deepScanInstructionSet.VisibleConversationCount;
        public IList<RecentlyAddedProspectModel> RecentlyAddedProspects => _checkOffHoursInstructionSet.RecentlyAddedProspects;
        public IList<SearchUrlProgressModel> GetUpdatedSearchUrls() => _networkingInstructionSet.GetUpdatedSearchUrls();
        public List<PersistPrimaryProspectModel> GetPersistPrimaryProspects() => _networkingInstructionSet.GetPersistPrimaryProspects();
        public IList<ConnectionSentModel> GetConnectionsSent() => _networkingInstructionSet.GetConnectionsSent();
        public bool GetMonthlySearchLimitReached() => _networkingInstructionSet.GetMonthlySearchLimitReached();
        public void ConnectWithProspectsForSearchUrl(IWebDriver webDriver, NetworkingMessageBody message, SearchUrlProgressModel searchUrlProgress, int totalResults)
        {
            _networkingInstructionSet.ConnectWithProspectsForSearchUrl(webDriver, message, searchUrlProgress, totalResults);
        }
        public void BeginDeepScanning(IWebDriver webDriver, IList<NetworkProspectModel> prospects, int visibleMessagesCount)
        {
            _deepScanInstructionSet.BeginDeepScanning(webDriver, prospects, visibleMessagesCount);
        }
        public bool ClearMessagingSearchCriteriaInteraction(IWebDriver webDriver) => _deepScanInstructionSet.ClearMessagingSearchCriteriaInteraction(webDriver);
        public bool GetVisibleConversationCountInteraction(IWebDriver webDriver) => _deepScanInstructionSet.GetVisibleConversationCountInteraction(webDriver);
        public void BeginCheckingForNewConnectionsFromOffHours(IWebDriver webDriver, CheckOffHoursNewConnectionsBody message) => _checkOffHoursInstructionSet.BeginCheckingForNewConnectionsFromOffHours(webDriver, message);
        public bool NoSearchResultsDisplayedInteraction(IWebDriver webDriver) => _networkingInstructionSet.NoSearchResultsDisplayedInteraction(webDriver);
        public bool GetTotalnumberOfSearchResultsInteraction(IWebDriver webDriver, SearchUrlProgressModel searchUrlProgress) => _networkingInstructionSet.GetTotalnumberOfSearchResultsInteraction(webDriver, searchUrlProgress);
        public void Add_UpdateSearchUrlProgressRequest(string searchUrlProgressId, int currentPage, string currentUrl, int totalResults, string currentWindowHandle) => _networkingInstructionSet.Add_UpdateSearchUrlProgressRequest(searchUrlProgressId, currentPage, currentUrl, totalResults, currentWindowHandle);
        public SentFollowUpMessageModel GetSentFollowUpMessage() => _followUpInstructionSet.GetSentFollowUpMessage();
        public void SendFollowUpMessage(IWebDriver webDriver, FollowUpMessageBody message) => _followUpInstructionSet.SendFollowUpMessage(webDriver, message);
    }
}
