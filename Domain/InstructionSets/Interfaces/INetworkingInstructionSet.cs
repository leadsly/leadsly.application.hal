﻿using Domain.Executors.AllInOneVirtualAssistant.Events;
using Domain.Models.Networking;
using Domain.Models.SendConnections;
using Domain.MQ.Messages;
using OpenQA.Selenium;
using System.Collections.Generic;

namespace Domain.InstructionSets.Interfaces
{
    public interface INetworkingInstructionSet
    {
        public int NumberOfConnectionsSent { get; set; }
        public bool MonthlySearchLimitReached { get; }
        public int TotalNumberOfSearchResults { get; }
        public IList<SearchUrlProgressModel> GetUpdatedSearchUrls();
        public event PersistPrimaryProspectsEventHandler PersistPrimaryProspects;
        public IList<ConnectionSentModel> GetConnectionsSent();
        public bool GetMonthlySearchLimitReached();
        public void ConnectWithProspectsForSearchUrl(IWebDriver webDriver, NetworkingMessageBody message, SearchUrlProgressModel searchUrlProgress, int totalResults);
        public bool GetTotalnumberOfSearchResultsInteraction(IWebDriver webDriver, SearchUrlProgressModel searchUrlProgress);
        public bool NoSearchResultsDisplayedInteraction(IWebDriver webDriver);
        public void Add_UpdateSearchUrlProgressRequest(string searchUrlProgressId, int currentPage, string currentUrl, int totalResults, string currentWindowHandle);
    }
}
