using Domain.Serializers.Interfaces;
using Leadsly.Application.Model.Campaigns;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public SearchUrlProgressResponse DeserializeSearchUrlsProgress(string json)
        {
            _logger.LogInformation("Deserializing SearchUrlProgressResponse");
            SearchUrlProgressResponse payload = default;
            try
            {
                payload = JsonConvert.DeserializeObject<SearchUrlProgressResponse>(json);
                _logger.LogDebug("Successfully deserialized SearchUrlProgressResponse");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deserialize SearchUrlProgressResponse");
            }
            return payload;
        }
    }
}
