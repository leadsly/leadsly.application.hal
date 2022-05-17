using Domain.Services.Interfaces;
using Leadsly.Application.Model.Requests.FromHal;
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
    public class TriggerPhaseService : ITriggerPhaseService
    {
        public TriggerPhaseService(ILogger<TriggerPhaseService> logger, HttpClient httpClient, IUrlService urlService)
        {
            _logger = logger;
            _urlService = urlService;
            _httpClient = httpClient;
        }

        private readonly ILogger<TriggerPhaseService> _logger;
        private readonly IUrlService _urlService;
        private readonly HttpClient _httpClient;

        public async Task<HttpResponseMessage> TriggerCampaignProspectListAsync(TriggerSendConnectionsRequest request, CancellationToken ct = default)
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
                        CampaignId = request.CampaignId,
                        UserId = request.UserId,
                        HalId = request.HalId
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

        public async Task<HttpResponseMessage> TriggerScanProspectsForRepliesAsync(TriggerScanProspectsForRepliesRequest request, CancellationToken ct = default)
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
                        HalId = request.HalId,
                        UserId = request.UserId
                    })
                };

                _logger.LogInformation("Sending request to trigger ScanProspectsForReplies phase");
                response = await _httpClient.SendAsync(req, ct);
                _logger.LogInformation("Successfully sent request to trigger ScanProspectsForReplies phase");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send request to trigger ScanProspectsForReplies phase");
            }

            return response;
        }

        public async Task<HttpResponseMessage> TriggerFollowUpMessageAsync(TriggerFollowUpMessageRequest request, CancellationToken ct = default)
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
                        HalId = request.HalId,
                        UserId = request.UserId
                    })
                };

                _logger.LogInformation("Sending request to trigger FollowUpMessagePhase");
                response = await _httpClient.SendAsync(req, ct);
                _logger.LogInformation("Successfully sent request to trigger FollowUpMessagePhase");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send request to trigger FollowUpMessagePhase");
            }

            return response;
        }
    }
}
