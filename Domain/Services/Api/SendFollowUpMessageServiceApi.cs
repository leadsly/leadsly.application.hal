using Domain.Models.Requests.FollowUpMessage;
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
                        MessageOrderNum = request.Item.MessageOrderNum,
                        ActualDeliveryDateTimeStamp = request.Item.ActualDeliveryDateTimeStamp
                    })
                };

                _logger.LogInformation("Sending request to process campaign prospect because we just sent them a follow up message");
                response = await _httpClient.SendAsync(req, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send request to update process campaign prospect with sent follow up message");
            }

            return response;
        }
    }
}
