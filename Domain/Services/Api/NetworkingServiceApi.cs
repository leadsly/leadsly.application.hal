using Domain.Models.Requests;
using Domain.Models.Requests.Networking;
using Domain.Models.Requests.ProspectList;
using Domain.Models.Requests.SendConnections;
using Domain.Services.Interfaces;
using Domain.Services.Interfaces.Api;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Services.Api
{
    public class NetworkingServiceApi : INetworkingServiceApi
    {
        public NetworkingServiceApi(ILogger<NetworkingServiceApi> logger, HttpClient httpClient, IUrlService urlService)
        {
            _logger = logger;
            _httpClient = httpClient;
            _urlService = urlService;
        }

        private readonly IUrlService _urlService;
        private readonly HttpClient _httpClient;
        private readonly ILogger<NetworkingServiceApi> _logger;

        public async Task<HttpResponseMessage> GetNetworkingMessagesAsync(GetNetworkingMessagesRequest request, CancellationToken ct = default)
        {
            string baseServerUrl = _urlService.GetBaseServerUrl(request.ServiceDiscoveryName, request.NamespaceName);

            HttpResponseMessage response = default;
            try
            {
                HttpRequestMessage req = new()
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri($"{baseServerUrl}/{request.RequestUrl}", UriKind.Absolute),
                };

                _logger.LogInformation("Sending request to get NetworkingMessages");
                response = await _httpClient.SendAsync(req, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send request to get NetworkingMessages");
            }

            return response;
        }

        public async Task<HttpResponseMessage> GetSearchUrlProgressAsync(GetSearchUrlProgressRequest request, CancellationToken ct = default)
        {
            string baseServerUrl = _urlService.GetBaseServerUrl(request.ServiceDiscoveryName, request.NamespaceName);

            HttpResponseMessage response = default;
            try
            {
                HttpRequestMessage req = new()
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri($"{baseServerUrl}/{request.RequestUrl}", UriKind.Absolute),
                };

                _logger.LogInformation("Sending request to get SearchUrlProgress");
                response = await _httpClient.SendAsync(req, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send request to get SearchUrlProgress");
            }

            return response;
        }

        public async Task<HttpResponseMessage> ProcessSentConnectionsAsync(ConnectionsSentRequest request, CancellationToken ct = default)
        {
            string baseServerUrl = _urlService.GetBaseServerUrl(request.ServiceDiscoveryName, request.NamespaceName);

            HttpResponseMessage response = default;
            try
            {
                HttpRequestMessage req = new()
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri($"{baseServerUrl}/{request.RequestUrl}", UriKind.Absolute),
                    Content = JsonContent.Create(new
                    {
                        Items = request.Items
                    })
                };

                _logger.LogInformation("Sending request to process my new network connections.");
                response = await _httpClient.SendAsync(req, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send reequest to process my new network connections.");
            }

            return response;
        }

        public async Task<HttpResponseMessage> UpdateMonthlySearchLimit(MonthlySearchLimitReachedRequest request, CancellationToken ct = default)
        {
            string baseServerUrl = _urlService.GetBaseServerUrl(request.ServiceDiscoveryName, request.NamespaceName);

            HttpResponseMessage response = default;
            try
            {
                HttpRequestMessage req = new()
                {
                    Method = HttpMethod.Patch,
                    RequestUri = new Uri($"{baseServerUrl}/{request.RequestUrl}", UriKind.Absolute),
                    Content = JsonContent.Create(new[]
                    {
                        new
                        {
                            op = "replace",
                            path = "/monthlySearchLimitReached",
                            value = request.MonthlySearchLimitReached.ToString()
                        }
                    })
                };

                _logger.LogInformation("Sending request to update monthly search limit.");
                response = await _httpClient.SendAsync(req, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send request to update monthly search limit.");
            }

            return response;
        }

        public async Task<HttpResponseMessage> UpdateSearchUrlAsync(UpdateSearchUrlProgressRequest request, CancellationToken ct = default)
        {
            string baseServerUrl = _urlService.GetBaseServerUrl(request.ServiceDiscoveryName, request.NamespaceName);

            HttpResponseMessage response = default;

            try
            {
                HttpRequestMessage req = new()
                {
                    Method = HttpMethod.Patch,
                    RequestUri = new Uri($"{baseServerUrl}/{request.RequestUrl}", UriKind.Absolute),
                    Content = JsonContent.Create(new[]
                    {
                        new
                        {
                            op = "replace",
                            path = "/windowHandleId",
                            value = request.Item.WindowHandleId
                        },
                        new
                        {
                            op = "replace",
                            path = "/lastPage",
                            value = request.Item.LastPage.ToString()
                        },
                        new
                        {
                            op = "replace",
                            path = "/lastProcessedProspect",
                            value = request.Item.LastProcessedProspect.ToString()
                        },
                        new
                        {
                            op = "replace",
                            path = "/searchUrl",
                            value = request.Item.SearchUrl
                        },
                        new
                        {
                            op = "replace",
                            path = "/startedCrawling",
                            value = request.Item.StartedCrawling.ToString()
                        },
                        new
                        {
                            op = "replace",
                            path = "/exhausted",
                            value = request.Item.Exhausted.ToString()
                        },
                        new
                        {
                            op = "replace",
                            path = "/lastActivityTimestamp",
                            value = request.Item.LastActivityTimestamp.ToString()
                        },
                        new
                        {
                            op = "replace",
                            path = "/totalSearchResults",
                            value = request.Item.TotalSearchResults.ToString()
                        }
                    })
                };

                _logger.LogInformation("Sending request to update sent connections url statuses");
                response = await _httpClient.SendAsync(req, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send request to update latest sent connections url statuses");
            }

            return response;
        }

        public async Task<HttpResponseMessage> ProcessProspectListAsync(CollectedProspectsRequest request, CancellationToken ct = default)
        {
            string baseServerUrl = _urlService.GetBaseServerUrl(request.ServiceDiscoveryName, request.NamespaceName);

            HttpResponseMessage response = default;
            try
            {
                HttpRequestMessage req = new()
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri($"{baseServerUrl}/{request.RequestUrl}", UriKind.Absolute),
                    Content = JsonContent.Create(new
                    {
                        request.CampaignProspectListId,
                        request.Items
                    })
                };

                _logger.LogInformation("Sending request to process primary prospects list.");
                response = await _httpClient.SendAsync(req, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send reequest to process primary prospects list.");
            }

            return response;
        }
    }
}
