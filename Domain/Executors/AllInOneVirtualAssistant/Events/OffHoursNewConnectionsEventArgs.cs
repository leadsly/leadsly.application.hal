using Domain.Models.MonitorForNewProspects;
using Domain.MQ.Messages;
using System.Collections.Generic;

namespace Domain.Executors.AllInOneVirtualAssistant.Events
{
    public class OffHoursNewConnectionsEventArgs
    {
        public OffHoursNewConnectionsEventArgs(PublishMessageBody message, IList<RecentlyAddedProspectModel> recentlyAddedProspects)
        {
            Message = message;
            RecentlyAddedProspects = recentlyAddedProspects;
        }

        public PublishMessageBody Message { get; set; }
        public IList<RecentlyAddedProspectModel> RecentlyAddedProspects { get; set; }
    }
}
