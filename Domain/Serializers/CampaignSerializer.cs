using Domain.Serializers.Interfaces;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.Responses;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;

namespace Domain.Serializers
{
    public class CampaignSerializer : ICampaignSerializer
    {
        public CampaignSerializer(ILogger<CampaignSerializer> logger)
        {
            _logger = logger;
        }

        private readonly ILogger<CampaignSerializer> _logger;
        public GetSentConnectionsUrlStatusPayload DeserializeSentConnectionsUrlStatuses(string json)
        {
            _logger.LogInformation("Deserializing SentConnectionUrlStatuses");
            GetSentConnectionsUrlStatusPayload payload = default;
            try
            {
                payload = JsonConvert.DeserializeObject<GetSentConnectionsUrlStatusPayload>(json);
                _logger.LogDebug("Successfully deserialized SentConnectionUrlStatuses");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deserialize SentConnectionUrlStatuses");
            }
            return payload;
        }

        public NewAcceptedCampaignProspectsPayload DeserializeNewAcceptedCampaignProspects(string json)
        {
            _logger.LogInformation("Deserializing NewAcceptedCampaignProspectsPayload");
            NewAcceptedCampaignProspectsPayload payload = default;
            try
            {
                payload = JsonConvert.DeserializeObject<NewAcceptedCampaignProspectsPayload>(json);
                _logger.LogDebug("Successfully deserialized NewAcceptedCampaignProspectsPayload");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deserialize NewAcceptedCampaignProspectsPayload");
            }
            return payload;
        }

        public SearchUrlProgressPayload DeserializeSearchUrlsProgress(string json)
        {
            _logger.LogInformation("Deserializing SearchUrlProgressPayload");
            SearchUrlProgressPayload payload = default;
            try
            {
                payload = JsonConvert.DeserializeObject<SearchUrlProgressPayload>(json);
                _logger.LogDebug("Successfully deserialized SearchUrlProgressPayload");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deserialize SearchUrlProgressPayload");
            }
            return payload;
        }
    }
}
