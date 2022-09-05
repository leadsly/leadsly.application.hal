using Leadsly.Application.Model.Campaigns;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Services.Interfaces
{
    public interface IMonitorForNewConnectionsService
    {
        Task ProcessRecentlyAddedProspectsAsync(IList<Models.RecentlyAddedProspect> requests, PublishMessageBody message, CancellationToken ct = default);
    }
}
