using Domain.Models.FollowUpMessage;
using Domain.Models.Requests.FollowUpMessage;
using Domain.MQ.Messages;
using Domain.Services.Interfaces;
using Domain.Services.Interfaces.Api;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Services
{
    public class FollowUpMessageService : IFollowUpMessageService
    {
        public FollowUpMessageService(ILogger<FollowUpMessageService> logger, ISendFollowUpMessageServiceApi api)
        {
            _logger = logger;
            _api = api;
        }

        private readonly ILogger<FollowUpMessageService> _logger;
        private readonly ISendFollowUpMessageServiceApi _api;

        public async Task ProcessSentFollowUpMessageAsync(SentFollowUpMessageModel item, FollowUpMessageBody message, CancellationToken ct = default)
        {
            SentFollowUpMessageRequest request = new()
            {
                NamespaceName = message.NamespaceName,
                ServiceDiscoveryName = message.ServiceDiscoveryName,
                RequestUrl = $"FollowUpMessage/{message.CampaignProspectId}/follow-up",
                Item = item
            };

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

        //public async Task<FollowUpMessagesResponse> GetFollowUpMessagesAsync(PublishMessageBody message, CancellationToken ct = default)
        //{
        //    GetFollowUpMessagesRequest request = new()
        //    {
        //        RequestUrl = $"FollowUpMessage/{message.HalId}/messages",
        //        NamespaceName = message.NamespaceName,
        //        ServiceDiscoveryName = message.ServiceDiscoveryName
        //    };

        //    HttpResponseMessage rawResponse = await _api.GetFollowUpMessagesAsync(request, ct);

        //    if (rawResponse == null)
        //    {
        //        _logger.LogError("Response from application server was null. The request was responsible for fetching FollowUpMessages");
        //        return null;
        //    }

        //    if (rawResponse.IsSuccessStatusCode == false)
        //    {
        //        string content = await rawResponse.Content.ReadAsStringAsync();
        //        _logger.LogError("Response from application server was not a successful status code. The request was responsible for getting FollowUpMessages. {content}", content);
        //        return null;
        //    }

        //    FollowUpMessagesResponse response = default;
        //    try
        //    {
        //        string json = await rawResponse.Content.ReadAsStringAsync();
        //        response = JsonConvert.DeserializeObject<FollowUpMessagesResponse>(json);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Failed to deserialize response from application server. The request was responsible for getting FollowUpMessages");
        //    }

        //    return response;
        //}
    }
}
