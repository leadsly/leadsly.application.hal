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
        public CampaignService(HttpClient httpClient, ILogger<CampaignService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        private readonly HttpClient _httpClient;
        private readonly ILogger<CampaignService> _logger;
        private const string HttpPrefix = "http://";

        public async Task<HttpResponseMessage> GetLatestSentConnectionsUrlStatusesAsync(SentConnectionsUrlStatusRequest request, CancellationToken ct = default)
        {
            string apiServerUrl = $"https://localhost:5001/{request.RequestUrl}"; //$"{HttpPrefix}{request.ServiceDiscoveryName}.{request.NamespaceName}";

            HttpResponseMessage response = default;

            try
            {
                HttpRequestMessage req = new()
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(apiServerUrl, UriKind.Absolute)
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

        public async Task<HttpResponseMessage> UpdateSendConnectionsUrlStatusesAsync(UpdateSentConnectionsUrlStatusRequest request, CancellationToken ct = default)
        {
            string apiServerUrl = $"https://localhost:5001/{request.RequestUrl}"; //$"{HttpPrefix}{request.ServiceDiscoveryName}.{request.NamespaceName}";

            HttpResponseMessage response = default;

            try
            {
                HttpRequestMessage req = new()
                {
                    Method = HttpMethod.Patch,
                    RequestUri = new Uri(apiServerUrl, UriKind.Absolute),
                    Content = JsonContent.Create(new
                    {
                        HalId = request.HalId,
                        SentConnectionsUrlStatuses = request.SentConnectionsUrlStatuses
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
            string apiServerUrl = $"https://localhost:5001/{request.RequestUrl}"; //$"{HttpPrefix}{request.ServiceDiscoveryName}.{request.NamespaceName}";

            HttpResponseMessage response = default;

            try
            {
                HttpRequestMessage req = new()
                {
                    Method = HttpMethod.Patch,
                    RequestUri = new Uri(apiServerUrl, UriKind.Absolute),
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
    }
}
