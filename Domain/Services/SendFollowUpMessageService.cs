using Domain.Models.Requests;
using Domain.Services.Interfaces;
using Domain.Services.Interfaces.Api;
using Leadsly.Application.Model.Campaigns;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Services
{
    public class SendFollowUpMessageService : ISendFollowUpMessageService
    {
        public SendFollowUpMessageService(ILogger<SendFollowUpMessageService> logger, ISendFollowUpMessageServiceApi api)
        {
            _logger = logger;
            _api = api;
        }

        private readonly ILogger<SendFollowUpMessageService> _logger;
        private readonly ISendFollowUpMessageServiceApi _api;

        public async Task ProcessSentFollowUpMessageAsync(SentFollowUpMessageRequest request, FollowUpMessageBody message, CancellationToken ct = default)
        {
            request.NamespaceName = message.NamespaceName;
            request.ServiceDiscoveryName = message.ServiceDiscoveryName;
            request.RequestUrl = $"FollowUpMessage/{request.CampaignProspectId}/follow-up";

            HttpResponseMessage response = await _api.ProcessSentFollowUpMessageAsync(request, ct);

            if (response == null)
            {
                _logger.LogError("Response from application server was null");
            }

            if (response.IsSuccessStatusCode == false)
            {
                string content = await response.Content.ReadAsStringAsync();
                _logger.LogError("Response from application server was not a successful status code. This request was responsible for updating campaign prospect who received a follow up message. Content {content}", content);
            }
        }
    }
}
