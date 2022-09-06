using Domain.Models.MonitorForNewProspects;
using Domain.Models.RabbitMQMessages;
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
