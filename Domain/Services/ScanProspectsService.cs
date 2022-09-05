using Domain.Services.Interfaces;
using Domain.Services.Interfaces.Api;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.Requests;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Services
{
    public class ScanProspectsService : IScanProspectsService
    {
        public ScanProspectsService(ILogger<ScanProspectsService> logger, IScanProspectsForRepliesServiceApi scanProspectsForRepliesServiceApi)
        {
            _logger = logger;
            _scanProspectsForRepliesServiceApi = scanProspectsForRepliesServiceApi;
        }

        private readonly ILogger<ScanProspectsService> _logger;
        private readonly IScanProspectsForRepliesServiceApi _scanProspectsForRepliesServiceApi;

        public async Task ProcessNewMessagesAsync(IList<NewMessageRequest> newMessageRequests, ScanProspectsForRepliesBody message, CancellationToken ct = default)
        {
            NewMessagesRequest request = new()
            {
                HalId = message.HalId,
                NamespaceName = message.NamespaceName,
                ServiceDiscoveryName = message.ServiceDiscoveryName,
                RequestUrl = $"ScanProspectsForReplies/{message.HalId}/prospects-replied",
                NewMessages = newMessageRequests
            };

            HttpResponseMessage response = await _scanProspectsForRepliesServiceApi.ProcessNewMessagesAsync(request, ct);

            if (response == null)
            {
                _logger.LogError("Response from application server was null. The request was responsible processing new messages from potential campaign prospects.");
            }

            if (response.IsSuccessStatusCode == false)
            {
                string content = await response.Content.ReadAsStringAsync();
                _logger.LogError("Response from application server was not a successful status code. The request was responsible processing new messages from potential campaign prospects. Content: {content}", content);
            }
        }
    }
}
