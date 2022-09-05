using Domain.Models.MonitorForNewProspects;
using Domain.Models.RabbitMQMessages;
using System;
using System.Collections.Generic;

namespace Domain.Executors.MonitorForNewConnections.Events
{
    public class NewRecentlyAddedProspectsDetectedEventArgs : EventArgs
    {
        public NewRecentlyAddedProspectsDetectedEventArgs(PublishMessageBody message, IList<RecentlyAddedProspect> newRecentlyAddedProspects)
        {
            Message = message;
            NewRecentlyAddedProspects = newRecentlyAddedProspects;
        }

        public PublishMessageBody Message { get; set; }
        public IList<RecentlyAddedProspect> NewRecentlyAddedProspects { get; set; }
    }
}