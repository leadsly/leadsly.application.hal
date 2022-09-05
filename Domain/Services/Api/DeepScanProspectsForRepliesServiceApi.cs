using Domain.Models.Requests.DeepScanProspectsForReplies;
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
    public class DeepScanProspectsForRepliesServiceApi : IDeepScanProspectsForRepliesServiceApi
    {
        public DeepScanProspectsForRepliesServiceApi(ILogger<DeepScanProspectsForRepliesServiceApi> logger, HttpClient httpClient, IUrlService urlService)
        {
            _logger = logger;
            _httpClient = httpClient;
            _urlService = urlService;
        }

        private readonly ILogger<DeepScanProspectsForRepliesServiceApi> _logger;
        private readonly HttpClient _httpClient;
        private readonly IUrlService _urlService;

        public async Task<HttpResponseMessage> GetAllProspectsFromActiveCampaignsAsync(AllNetworkProspectsRequest request, CancellationToken ct = default)
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

                _logger.LogInformation("Sending request to retrieve all network prospects from active campaigns.");
                response = await _httpClient.SendAsync(req, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send request to retrieve all network prospects from active campaigns");
            }

            return response;
        }

        public async Task<HttpResponseMessage> ProcessProspectsRepliedAsync(ProspectsRepliedRequest request, CancellationToken ct = default)
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

                _logger.LogInformation("Sending request to process campaign prospects that have replied to our campaign messages.");
                response = await _httpClient.SendAsync(req, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send request to update campaign prospects replied property and record their response message");
            }

            return response;
        }
    }
}
