using Domain.Services.Interfaces;
using Domain.Services.Interfaces.Api;
using Leadsly.Application.Model.Requests;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Services.Api
{
    public class ScanProspectsForRepliesServiceApi : IScanProspectsForRepliesServiceApi
    {
        public ScanProspectsForRepliesServiceApi(ILogger<ScanProspectsForRepliesServiceApi> logger, HttpClient httpClient, IUrlService urlService)
        {
            _logger = logger;
            _httpClient = httpClient;
            _urlService = urlService;
        }

        private readonly ILogger<ScanProspectsForRepliesServiceApi> _logger;
        private readonly HttpClient _httpClient;
        private readonly IUrlService _urlService;

        public async Task<HttpResponseMessage> ProcessNewMessagesAsync(NewMessagesRequest request, CancellationToken ct = default)
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
                        ProspectsReplied = request.NewMessages
                    })
                };

                _logger.LogInformation("Sending to process new messages. These may be responses from campaign prospects.");
                response = await _httpClient.SendAsync(req, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send request to process new messages from potential campaign prospects");
            }

            return response;
        }
    }
}
