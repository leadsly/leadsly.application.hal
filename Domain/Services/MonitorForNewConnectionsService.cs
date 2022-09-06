using Domain.Models.RabbitMQMessages;
using Domain.Models.Requests.MonitorForNewConnections;
using Domain.Services.Interfaces;
using Domain.Services.Interfaces.Api;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using RecentlyAddedProspectModel = Domain.Models.MonitorForNewProspects.RecentlyAddedProspectModel;

namespace Domain.Services
{
    public class MonitorForNewConnectionsService : IMonitorForNewConnectionsService
    {
        public MonitorForNewConnectionsService(ILogger<MonitorForNewConnectionsService> logger, IMonitorProspectsForNewConnectionsServiceApi api)
        {
            _api = api;
            _logger = logger;
        }

        private readonly ILogger<MonitorForNewConnectionsService> _logger;
        private readonly IMonitorProspectsForNewConnectionsServiceApi _api;

        public async Task ProcessRecentlyAddedProspectsAsync(IList<RecentlyAddedProspectModel> items, PublishMessageBody message, CancellationToken ct = default)
        {
            RecentlyAddedProspectsRequest request = new()
            {
                NamespaceName = message.NamespaceName,
                ServiceDiscoveryName = message.ServiceDiscoveryName,
                RequestUrl = $"MonitorForNewProspects/{message.HalId}/new-prospects",
                Items = items,
                ApplicationUserId = message.UserId
            };

            HttpResponseMessage response = await _api.ProcessNewlyDetectedProspectsAsync(request);

            if (response == null)
            {
                _logger.LogError("Response from application server was null. The request was responsible for processing newly accepted connections");
            }

            if (response.IsSuccessStatusCode == false)
            {
                string content = await response.Content.ReadAsStringAsync();
                _logger.LogError("Response from application server was not a successfull status code. The request was responsible processing newly accepted connections. Response content {content}", content);
            }
        }
    }
}
