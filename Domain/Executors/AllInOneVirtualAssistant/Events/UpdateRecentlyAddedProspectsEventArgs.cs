using Domain.Models.MonitorForNewProspects;
using Domain.MQ.Messages;
using System;
using System.Collections.Generic;

namespace Domain.Executors.AllInOneVirtualAssistant.Events
{
    public class UpdateRecentlyAddedProspectsEventArgs : EventArgs
    {
        public UpdateRecentlyAddedProspectsEventArgs(PublishMessageBody message, IList<RecentlyAddedProspectModel> newRecentlyAddedProspects, int totalConnectionsCount)
        {
            Message = message;
            TotalConnectionsCount = totalConnectionsCount;
            NewRecentlyAddedProspects = newRecentlyAddedProspects;
        }

        public PublishMessageBody Message { get; set; }
        public int TotalConnectionsCount { get; set; }
        public IList<RecentlyAddedProspectModel> NewRecentlyAddedProspects { get; set; }
    }
}
