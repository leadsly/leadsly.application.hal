using Leadsly.Application.Model.Campaigns;
using System;
using System.Collections.Generic;

namespace Domain.Executors.MonitorForNewConnections.Events
{
    public class NewRecentlyAddedProspectsDetectedEventArgs : EventArgs
    {
        public NewRecentlyAddedProspectsDetectedEventArgs(PublishMessageBody message, IList<Models.RecentlyAddedProspect> newRecentlyAddedProspects)
        {
            Message = message;
            NewRecentlyAddedProspects = newRecentlyAddedProspects;
        }

        public PublishMessageBody Message { get; set; }
        public IList<Models.RecentlyAddedProspect> NewRecentlyAddedProspects { get; set; }
    }
}