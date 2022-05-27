using Domain.Models.Requests;
using Domain.Services.Interfaces;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.Requests;
using Leadsly.Application.Model.Requests.FromHal;
using Leadsly.Application.Model.Responses;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Services
{
    public class CampaignService : ICampaignService
    {
        public CampaignService(HttpClient httpClient, ILogger<CampaignService> logger, IUrlService urlService)
        {
            _httpClient = httpClient;
            _logger = logger;
            _urlService = urlService;
        }

        private readonly HttpClient _httpClient;
        private readonly ILogger<CampaignService> _logger;
        private readonly IUrlService _urlService;
        private const string HttpPrefix = "http://";

        public async Task<HttpResponseMessage> GetLatestSentConnectionsUrlStatusesAsync(SearchUrlDetailsRequest request, CancellationToken ct = default)
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

                _logger.LogInformation("Sending request to get latest sent connections url status");
                response = await _httpClient.SendAsync(req, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send request to get latest sent connections url statuses");
            }

            return response;
        }

        public async Task<HttpResponseMessage> GetSearchUrlProgressAsync(SearchUrlProgressRequest request, CancellationToken ct = default)
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

        public async Task<HttpResponseMessage> UpdateSendConnectionsUrlStatusesAsync(UpdateSearchUrlDetailsRequest request, CancellationToken ct = default)
        {
            string baseServerUrl = _urlService.GetBaseServerUrl(request.ServiceDiscoveryName, request.NamespaceName);

            HttpResponseMessage response = default;

            try
            {
                HttpRequestMessage req = new()
                {
                    Method = HttpMethod.Patch,
                    RequestUri = new Uri($"{baseServerUrl}/{request.RequestUrl}", UriKind.Absolute),
                    Content = JsonContent.Create(new
                    {
                        HalId = request.HalId,
                        SentConnectionsUrlStatuses = request.SearchUrlDetailsRequests
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

        public async Task<HttpResponseMessage> MarkCampaignExhausted(MarkCampaignExhaustedRequest request, CancellationToken ct = default)
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
                            path = "/active",
                            value = "false"
                        },
                        new
                        {
                            op = "replace",
                            path = "/expired",
                            value = "true"
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

        public async Task<HttpResponseMessage> UpdateSearchUrlProgressAsync(UpdateSearchUrlProgressRequest request, CancellationToken ct = default)
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
                            value = request.WindowHandleId
                        },
                        new
                        {
                            op = "replace",
                            path = "/lastPage",
                            value = request.LastPage.ToString()
                        },
                        new
                        {
                            op = "replace",
                            path = "/lastProcessedProspect",
                            value = request.LastProcessedProspect.ToString()
                        },
                        new
                        {
                            op = "replace",
                            path = "/searchUrl",
                            value = request.SearchUrl
                        },
                        new
                        {
                            op = "replace",
                            path = "/startedCrawling",
                            value = request.StartedCrawling.ToString()
                        },
                        new
                        {
                            op = "replace",
                            path = "/exhausted",
                            value = request.Exhausted.ToString()
                        },
                        new
                        {
                            op = "replace",
                            path = "/lastActivityTimestamp",
                            value = request.LastActivityTimestamp.ToString()
                        },
                        new
                        {
                            op = "replace",
                            path = "/totalSearchResults",
                            value = request.TotalSearchResults.ToString()
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

    }
}
