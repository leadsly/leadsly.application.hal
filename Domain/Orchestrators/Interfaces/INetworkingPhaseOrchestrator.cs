﻿using Domain.Models.ProspectList;
using Domain.Models.SendConnections;
using Domain.MQ.Messages;
using System.Collections.Generic;

namespace Domain.Orchestrators.Interfaces
{
    public interface INetworkingPhaseOrchestrator
    {
        public IList<Models.Networking.SearchUrlProgressModel> GetUpdatedSearchUrls();
        public List<PersistPrimaryProspectModel> GetPersistPrimaryProspects();
        public IList<ConnectionSentModel> GetConnectionsSent();
        public bool GetMonthlySearchLimitReached();
        void Execute(NetworkingMessageBody message, IList<Models.Networking.SearchUrlProgressModel> searchUrlsProgress);
    }
}