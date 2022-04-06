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
            GetSentConnectionsUrlStatusPayload payload = default;
            try
            {
                payload = JsonConvert.DeserializeObject<GetSentConnectionsUrlStatusPayload>(json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deserialize sent connections url statuses");
            }
            return payload;
        }
    }
}
