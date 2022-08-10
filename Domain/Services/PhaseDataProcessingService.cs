using Domain.Services.Interfaces;
using Leadsly.Application.Model.Requests;
using Leadsly.Application.Model.Requests.FromHal;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Services
{
    public class PhaseDataProcessingService : IPhaseDataProcessingService
    {
        public PhaseDataProcessingService(HttpClient httpClient, ILogger<PhaseDataProcessingService> logger, IUrlService urlService)
        {
            _httpClient = httpClient;
            _logger = logger;
            _urlService = urlService;
        }

        private readonly HttpClient _httpClient;
        private readonly ILogger<PhaseDataProcessingService> _logger;
        private readonly IUrlService _urlService;

        public async Task<HttpResponseMessage> ProcessNewConnectionsAsync(NewProspectConnectionRequest request, CancellationToken ct = default)
        {
            //string apiServerUrl = "http://localhost:5010"; //; request.ApiServerUrl;

            //HttpRequestMessage req = new()
            //{
            //    Method = HttpMethod.Post,
            //    RequestUri = new Uri(apiServerUrl, UriKind.Relative),
            //    Content = JsonContent.Create(new
            //    {
            //        // NewConnectionProspects = request.NewConnectionProspects,
            //        HalId = request.HalId
            //    })
            //};

            //HttpResponseMessage response = default;
            //try
            //{
            //    _logger.LogInformation("Sending request to process my new network connections.");
            //    response = await _httpClient.SendAsync(req, ct);
            //}
            //catch(Exception ex)
            //{
            //    _logger.LogError(ex, "Failed to send reequest to process my new network connections.");
            //}

            //return response;

            return null;
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
                        PrimaryProspectListId = request.PrimaryProspectListId,
                        CampaignProspectListId = request.CampaignProspectListId,
                        UserId = request.UserId,
                        HalId = request.HalId,
                        CampaignId = request.CampaignId,
                        Prospects = request.Prospects,

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

        public async Task<HttpResponseMessage> ProcessContactedCampaignProspectListAsync(CampaignProspectListRequest request, CancellationToken ct = default)
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
                        UserId = request.UserId,
                        HalId = request.HalId,
                        CampaignId = request.CampaignId,
                        CampaignProspects = request.CampaignProspects
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
        public async Task<HttpResponseMessage> ProcessNewlyAcceptedProspectsAsync(NewProspectsConnectionsAcceptedRequest request, CancellationToken ct = default)
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
                        ApplicationUserId = request.ApplicationUserId,
                        NewAcceptedProspectsConnections = request.NewAcceptedProspectsConnections
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
                        HalId = request.HalId,
                        ProspectsReplied = request.ProspectsReplied
                    })
                };

                _logger.LogInformation("Sending request to campaign prospects for replied and response message");
                response = await _httpClient.SendAsync(req, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send request to update campaign prospects replied property and record their response message");
            }

            return response;
        }

        public async Task<HttpResponseMessage> ProcessFollowUpMessageSentAsync(FollowUpMessageSentRequest request, CancellationToken ct = default)
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
                        CampaignProspectId = request.CampaignProspectId,
                        ProspectName = request.ProspectName,
                        MessageOrderNum = request.MessageOrderNum,
                        MessageSentTimeStamp = request.MessageSentTimestamp
                    })
                };

                _logger.LogInformation("Sending request to campaign prospects for replied and response message");
                response = await _httpClient.SendAsync(req, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send request to update campaign prospects replied property and record their response message");
            }

            return response;
        }

        public async Task<HttpResponseMessage> MarkProspectListCompleteAsync(MarkProspectListPhaseCompleteRequest request, CancellationToken ct = default)
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
                            path = "/completed",
                            value = "true"
                        }
                    })
                };

                _logger.LogInformation("Sending request to mark prospect list phase as completed");
                response = await _httpClient.SendAsync(req, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send request to mark prospect list phase as completed");
            }

            return response;
        }

        public async Task<HttpResponseMessage> UpdateSocialAccountMonthlySearchLimitAsync(UpdateSocialAccountRequest request, CancellationToken ct = default)
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
                            value = "true"
                        }
                    })
                };

                _logger.LogInformation("Sending request to update social account 'MonthlySearchLimitReached' property");
                response = await _httpClient.SendAsync(req, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send request to update social account 'MonthlySearchLimitReached' property");
            }

            return response;
        }
    }

}
