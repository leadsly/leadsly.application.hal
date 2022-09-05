using Domain.Models.Requests.MonitorForNewConnections;
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
    public class MonitorProspectsForNewConnectionsServiceApi : IMonitorProspectsForNewConnectionsServiceApi
    {
        public MonitorProspectsForNewConnectionsServiceApi(ILogger<MonitorProspectsForNewConnectionsServiceApi> logger, HttpClient httpClient, IUrlService urlService)
        {
            _logger = logger;
            _httpClient = httpClient;
            _urlService = urlService;
        }

        private readonly ILogger<MonitorProspectsForNewConnectionsServiceApi> _logger;
        private readonly HttpClient _httpClient;
        private readonly IUrlService _urlService;

        public async Task<HttpResponseMessage> ProcessNewlyDetectedProspectsAsync(RecentlyAddedProspectsRequest request, CancellationToken ct = default)
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
                        ApplicationUserId = request.ApplicationUserId,
                        Items = request.Items
                    })
                };

                _logger.LogInformation("Sending request to process newly accepted connections");
                response = await _httpClient.SendAsync(req, ct);
                _logger.LogInformation("Successfully sent request to process newly accepted connections");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send request to process newly accepted connections");
            }

            return response;
        }
    }
}
