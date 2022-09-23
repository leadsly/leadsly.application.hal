using Domain.Models.DeepScanProspectsForReplies;
using Domain.Models.FollowUpMessage;
using Domain.Models.MonitorForNewProspects;
using Domain.Models.Networking;
using Domain.Models.ProspectList;
using Domain.Models.SendConnections;
using Domain.MQ.Messages;
using OpenQA.Selenium;
using System.Collections.Generic;

namespace Domain.Facades.Interfaces
{
    public interface IAllInOneInstructionsSetFacade
    {
        public IList<ProspectRepliedModel> ProspectsThatReplied { get; }
        public int VisibleConversationCount { get; }
        public IList<RecentlyAddedProspectModel> RecentlyAddedProspects { get; }
        public int NumberOfConnectionsSent { get; set; }
        public bool MonthlySearchLimitReached { get; }
        public IList<SearchUrlProgressModel> UpdatedSearchUrlsProgress { get; }
        public IList<SearchUrlProgressModel> GetUpdatedSearchUrls();
        public List<PersistPrimaryProspectModel> GetPersistPrimaryProspects();
        public IList<ConnectionSentModel> GetConnectionsSent();
        public bool GetMonthlySearchLimitReached();
        public void ConnectWithProspectsForSearchUrl(IWebDriver webDriver, NetworkingMessageBody message, SearchUrlProgressModel searchUrlProgress, int totalResults);
        public void BeginDeepScanning(IWebDriver webDriver, IList<NetworkProspectModel> prospects, int visibleMessagesCount);
        public bool ClearMessagingSearchCriteriaInteraction(IWebDriver webDriver);
        public bool GetVisibleConversationCountInteraction(IWebDriver webDriver);
        public void BeginCheckingForNewConnectionsFromOffHours(IWebDriver webDriver, CheckOffHoursNewConnectionsBody message);
        public bool NoSearchResultsDisplayedInteraction(IWebDriver webDriver);
        public bool GetTotalnumberOfSearchResultsInteraction(IWebDriver webDriver, SearchUrlProgressModel searchUrlProgress);
        public void Add_UpdateSearchUrlProgressRequest(string searchUrlProgressId, int currentPage, string currentUrl, int totalResults, string currentWindowHandle);
        public SentFollowUpMessageModel GetSentFollowUpMessage();
        public void SendFollowUpMessage(IWebDriver webDriver, FollowUpMessageBody message);
    }
}
