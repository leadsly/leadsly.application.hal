using Domain.Models.MonitorForNewProspects;
using Domain.MQ.Messages;
using System;
using System.Collections.Generic;

namespace Domain.Executors.MonitorForNewConnections.Events
{
    public class NewRecentlyAddedProspectsDetectedEventArgs : EventArgs
    {
        public NewRecentlyAddedProspectsDetectedEventArgs(PublishMessageBody message, IList<RecentlyAddedProspectModel> newRecentlyAddedProspects)
        {
            Message = message;
            NewRecentlyAddedProspects = newRecentlyAddedProspects;
        }

        public PublishMessageBody Message { get; set; }
        public IList<RecentlyAddedProspectModel> NewRecentlyAddedProspects { get; set; }
    }
}