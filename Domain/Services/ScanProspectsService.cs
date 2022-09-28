using Domain.Models.Requests.ScanProspectsForreplies;
using Domain.Models.ScanProspectsForReplies;
using Domain.MQ.Messages;
using Domain.Services.Interfaces;
using Domain.Services.Interfaces.Api;
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

        public async Task ProcessNewMessagesAsync(IList<NewMessageModel> items, PublishMessageBody message, CancellationToken ct = default)
        {
            NewMessagesRequest request = new()
            {
                NamespaceName = message.NamespaceName,
                ServiceDiscoveryName = message.ServiceDiscoveryName,
                RequestUrl = $"ScanProspectsForReplies/{message.HalId}/prospects-replied",
                Items = items
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
