using Domain.Models.ProspectList;
using Domain.Models.RabbitMQMessages;
using Domain.Models.Requests;
using Domain.Models.Requests.Networking;
using Domain.Models.Requests.ProspectList;
using Domain.Models.Requests.SendConnections;
using Domain.Models.Responses;
using Domain.Models.SendConnections;
using Domain.Services.Interfaces;
using Domain.Services.Interfaces.Api;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Services
{
    public class NetworkingService : INetworkingService
    {
        public NetworkingService(ILogger<NetworkingService> logger, ITimestampService timestampService, INetworkingServiceApi networkingServiceApi)
        {
            _logger = logger;
            _networkingServiceApi = networkingServiceApi;
            _timestampService = timestampService;
        }

        private readonly INetworkingServiceApi _networkingServiceApi;
        private readonly ILogger<NetworkingService> _logger;
        private readonly ITimestampService _timestampService;

        public async Task<GetSearchUrlProgressResponse> GetSearchUrlProgressAsync(NetworkingMessageBody message, CancellationToken ct = default)
        {
            GetSearchUrlProgressRequest request = new()
            {
                RequestUrl = $"Networking/{message.CampaignId}/url",
                NamespaceName = message.NamespaceName,
                ServiceDiscoveryName = message.ServiceDiscoveryName
            };

            HttpResponseMessage rawResponse = await _networkingServiceApi.GetSearchUrlProgressAsync(request, ct);

            if (rawResponse == null)
            {
                _logger.LogError("Response from application server was null. The request was responsible for fetching search url progress");
                return null;
            }

            if (rawResponse.IsSuccessStatusCode == false)
            {
                string content = await rawResponse.Content.ReadAsStringAsync();
                _logger.LogError("Response from application server was not a successful status code. The request was responsible for getting search url progress. {content}", content);
                return null;
            }

            GetSearchUrlProgressResponse response = default;
            try
            {
                string json = await rawResponse.Content.ReadAsStringAsync();
                response = JsonConvert.DeserializeObject<GetSearchUrlProgressResponse>(json);
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to deserialize response from application server. The request was responsible for getting search url progress");
            }

            return response;
        }

        public async Task ProcessSentConnectionsAsync(IList<ConnectionSent> items, NetworkingMessageBody message, CancellationToken ct = default)
        {
            ConnectionsSentRequest request = new()
            {
                Items = items,
                RequestUrl = $"SendConnections/{message.CampaignId}/prospects",
                NamespaceName = message.NamespaceName,
                ServiceDiscoveryName = message.ServiceDiscoveryName,
            };

            HttpResponseMessage response = await _networkingServiceApi.ProcessSentConnectionsAsync(request, ct);
            if (response == null)
            {
                _logger.LogError("Response from application server was null. The request was responsible for saving primary prospects to the database");
            }

            if (response.IsSuccessStatusCode == false)
            {
                string content = await response.Content.ReadAsStringAsync();
                _logger.LogError("Response from application server was not a successfull status code. The request was responsible for saving primary prospects to the database. Response content {content}", content);
            }
        }

        public async Task UpdateSearchUrlsAsync(IList<Domain.Models.Networking.SearchUrlProgress> items, NetworkingMessageBody message, CancellationToken ct = default)
        {
            IList<Task<HttpResponseMessage>> reqs = items.Select(x =>
            {
                UpdateSearchUrlProgressRequest request = new()
                {
                    Item = x,
                    RequestUrl = $"Networking/{x.SearchUrlProgressId}/url",
                    NamespaceName = message.NamespaceName,
                    ServiceDiscoveryName = message.ServiceDiscoveryName
                };

                return _networkingServiceApi.UpdateSearchUrlAsync(request, ct);
            }).ToList();

            await Task.WhenAll(reqs);

            IList<HttpResponseMessage> responses = reqs.Select(t => t.Result).ToList();

            foreach (HttpResponseMessage response in responses)
            {
                if (response == null)
                {
                    _logger.LogError("Response from application server was null. The request was responsible for updating SearchUrlProgress");
                }

                if (response.IsSuccessStatusCode == false)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Response from application server was not a successful status code. The request was responsible for updating SearchUrlProgress. Content was {content}", content);
                }
            }
        }

        public async Task ProcessProspectListAsync(IList<PersistPrimaryProspect> items, NetworkingMessageBody message, CancellationToken ct = default)
        {
            CollectedProspectsRequest request = new()
            {
                CampaignProspectListId = message.CampaignProspectListId,
                Items = items,
                RequestUrl = $"ProspectList/{message.HalId}",
                NamespaceName = message.NamespaceName,
                ServiceDiscoveryName = message.ServiceDiscoveryName
            };

            HttpResponseMessage response = await _networkingServiceApi.ProcessProspectListAsync(request, ct);
            if (response == null)
            {
                _logger.LogError("Response from application server was null. The request was responsible for saving primary prospects to the database");
            }

            if (response.IsSuccessStatusCode == false)
            {
                string content = await response.Content.ReadAsStringAsync();
                _logger.LogError("Response from application server was not a successfull status code. The request was responsible for saving primary prospects to the database. Content was {content}", content);
            }
        }

        public async Task UpdateMonthlySearchLimitAsync(bool limitReached, NetworkingMessageBody message, CancellationToken ct = default)
        {
            MonthlySearchLimitReachedRequest request = new()
            {
                NamespaceName = message.NamespaceName,
                ServiceDiscoveryName = message.ServiceDiscoveryName,
                MonthlySearchLimitReached = limitReached,
                RequestUrl = $"SocialAccounts/{message.SocialAccountId}"
            };

            HttpResponseMessage response = await _networkingServiceApi.UpdateMonthlySearchLimit(request, ct);
            if (response == null)
            {
                _logger.LogError("Response from application server was null. The request was responsible for updating users search limit.");
            }

            if (response.IsSuccessStatusCode == false)
            {
                string content = await response.Content.ReadAsStringAsync();
                _logger.LogError("Response from application server was not a successfull status code. The request was responsible for updating users search limit. Content: {content}", content);
            }
        }
    }
}
