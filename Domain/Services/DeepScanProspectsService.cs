using Domain.Models.DeepScanProspectsForReplies;
using Domain.Models.Requests.DeepScanProspectsForReplies;
using Domain.Models.Responses;
using Domain.MQ.Messages;
using Domain.Services.Interfaces;
using Domain.Services.Interfaces.Api;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Services
{
    public class DeepScanProspectsService : IDeepScanProspectsService
    {
        public DeepScanProspectsService(ILogger<DeepScanProspectsService> logger, IDeepScanProspectsForRepliesServiceApi serviceApi)
        {
            _logger = logger;
            _serviceApi = serviceApi;
        }

        private readonly ILogger<DeepScanProspectsService> _logger;
        private readonly IDeepScanProspectsForRepliesServiceApi _serviceApi;

        public async Task<NetworkProspectsResponse> GetAllProspectsFromActiveCampaignsAsync(DeepScanProspectsForRepliesBody message, CancellationToken ct = default)
        {
            NetworkProspectsResponse networkProspects = default;

            AllNetworkProspectsRequest request = new()
            {
                NamespaceName = message.NamespaceName,
                ServiceDiscoveryName = message.ServiceDiscoveryName,
                RequestUrl = $"DeepScanProspectsForReplies/{message.HalId}/all-network-prospects",
            };

            HttpResponseMessage response = await _serviceApi.GetAllProspectsFromActiveCampaignsAsync(request, ct);

            if (response == null)
            {
                _logger.LogError("Failed to retrieve all prospects in our network from active campaigns");
                return networkProspects;
            }

            if (response.IsSuccessStatusCode == false)
            {

                string content = await response.Content.ReadAsStringAsync();
                _logger.LogError("Response from application server was not successful. Content {content}", content);
                return networkProspects;
            }

            string json = await response.Content.ReadAsStringAsync();
            try
            {
                networkProspects = JsonConvert.DeserializeObject<NetworkProspectsResponse>(json);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to deserilize NetworkProspects.");
            }

            return networkProspects;
        }

        public async Task ProcessCampaignProspectsThatRepliedAsync(IList<ProspectRepliedModel> prospects, DeepScanProspectsForRepliesBody message, CancellationToken ct = default)
        {
            ProspectsRepliedRequest request = new()
            {
                NamespaceName = message.NamespaceName,
                ServiceDiscoveryName = message.ServiceDiscoveryName,
                RequestUrl = $"DeepScanProspectsForReplies/{message.HalId}",
                Items = prospects
            };

            HttpResponseMessage response = await _serviceApi.ProcessProspectsRepliedAsync(request, ct);

            if (response == null)
            {
                _logger.LogError("Response from application server was null");
            }

            if (response.IsSuccessStatusCode == false)
            {
                string content = await response.Content.ReadAsStringAsync();
                _logger.LogError("Response from application server was not a successful status code. This request was responsible for processing all of the campaign prospects that have replied to our message. Content {content}", content);
            }
        }
    }
}
