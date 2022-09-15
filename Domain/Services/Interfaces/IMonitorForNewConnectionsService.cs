using Domain.Models.MonitorForNewProspects;
using Domain.MQ.Messages;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Services.Interfaces
{
    public interface IMonitorForNewConnectionsService
    {
        Task ProcessRecentlyAddedProspectsAsync(IList<RecentlyAddedProspectModel> requests, PublishMessageBody message, CancellationToken ct = default);
    }
}
