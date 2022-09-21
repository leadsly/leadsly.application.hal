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
    public class AllInOneVirtualAssistantServiceApi : IAllInOneVirtualAssistantServiceApi
    {
        public AllInOneVirtualAssistantServiceApi(
            ILogger<AllInOneVirtualAssistantService> logger,
            HttpClient httpClient,
            IUrlService urlService)
        {
            _logger = logger;
            _httpClient = httpClient;
            _urlService = urlService;
        }

        private readonly ILogger<AllInOneVirtualAssistantService> _logger;
        private readonly HttpClient _httpClient;
        private readonly IUrlService _urlService;

        public async Task<HttpResponseMessage> GetAllPreviouslyConnectedNetworkProspectsAsync(PreviouslyConnectedNetworkProspectsRequest request, CancellationToken ct = default)
        {
            string baseServerUrl = _urlService.GetBaseServerUrl(request.ServiceDiscoveryName, request.NamespaceName);

            HttpResponseMessage response = default;
            try
            {
                HttpRequestMessage req = new()
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri($"{baseServerUrl}/{request.RequestUrl}", UriKind.Absolute)
                };

                _logger.LogInformation("Sending request to get all previously connected prospects. These are the prospects that appear on the connections page where it lists all recently connected");
                response = await _httpClient.SendAsync(req, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send request to get all recently connected prospects");
            }

            return response;
        }

        public async Task<HttpResponseMessage> UpdatePreviouslyConnectedNetworkProspectsAsync(UpdateCurrentConnectedNetworkProspectsRequest request, CancellationToken ct = default)
        {
            string baseServerUrl = _urlService.GetBaseServerUrl(request.ServiceDiscoveryName, request.NamespaceName);

            HttpResponseMessage response = default;
            try
            {
                HttpRequestMessage req = new()
                {
                    Method = HttpMethod.Put,
                    RequestUri = new Uri($"{baseServerUrl}/{request.RequestUrl}", UriKind.Absolute),
                    Content = JsonContent.Create(new
                    {
                        Items = request.Items,
                        TotalConnectionsCount = request.PreviousTotalConnectionsCount
                    })
                };

                _logger.LogInformation("Sending request to update recently added prospects");
                response = await _httpClient.SendAsync(req, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send request to update recently added prospects");
            }

            return response;
        }
    }
}
