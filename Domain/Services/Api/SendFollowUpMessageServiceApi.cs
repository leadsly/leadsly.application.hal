using Domain.Models.Requests;
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
    public class SendFollowUpMessageServiceApi : ISendFollowUpMessageServiceApi
    {
        public SendFollowUpMessageServiceApi(HttpClient httpClient, ILogger<SendFollowUpMessageServiceApi> logger, IUrlService urlService)
        {
            _urlService = urlService;
            _httpClient = httpClient;
            _logger = logger;
        }

        private readonly IUrlService _urlService;
        private readonly HttpClient _httpClient;
        private readonly ILogger<SendFollowUpMessageServiceApi> _logger;

        public async Task<HttpResponseMessage> ProcessSentFollowUpMessageAsync(SentFollowUpMessageRequest request, CancellationToken ct = default)
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
                        CampaignProspectId = request.CampaignProspectId,
                        MessageOrderNum = request.MessageOrderNum,
                        MessageSentTimeStamp = request.ActualDeliveryDateTimeStamp
                    })
                };

                _logger.LogInformation("Sending request to process campaign prospect {0} because we just sent them a follow up message", request.CampaignProspectId);
                response = await _httpClient.SendAsync(req, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send request to update process campaign prospect {0} with sent follow up message", request.CampaignProspectId);
            }

            return response;
        }
    }
}
