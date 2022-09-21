using Domain.Models.Networking;
using Domain.Models.ProspectList;
using Domain.Models.SendConnections;
using Domain.MQ.Messages;
using OpenQA.Selenium;
using System.Collections.Generic;

namespace Domain.InstructionSets.Interfaces
{
    public interface IConnectWithProspectsForSearchUrlInstructionSet
    {
        public int NumberOfConnectionsSent { get; set; }
        public bool MonthlySearchLimitReached { get; }
        public IList<SearchUrlProgressModel> UpdatedSearchUrlsProgress { get; }
        public IList<SearchUrlProgressModel> GetUpdatedSearchUrls();
        public List<PersistPrimaryProspectModel> GetPersistPrimaryProspects();
        public IList<ConnectionSentModel> GetConnectionsSent();
        public bool GetMonthlySearchLimitReached();
        public void ConnectWithProspectsForSearchUrl(IWebDriver webDriver, NetworkingMessageBody message, SearchUrlProgressModel searchUrlProgress, int totalResults);
    }
}
