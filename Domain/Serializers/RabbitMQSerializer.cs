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
    public class RabbitMQSerializer : IRabbitMQSerializer
    {
        public RabbitMQSerializer(ILogger<RabbitMQSerializer> logger)
        {
            _logger = logger;
        }

        private readonly ILogger<RabbitMQSerializer> _logger;

        public ConnectionWithdrawBody DeserializeConnectionWithdrawBody(string body)
        {
            throw new NotImplementedException();
        }

        public FollowUpMessageBody DeserializeFollowUpMessagesBody(string body)
        {
            _logger.LogInformation("Deserializing FollowUpMessageBody");
            FollowUpMessageBody followUpMessageBody = null;
            try
            {
                followUpMessageBody = JsonConvert.DeserializeObject<FollowUpMessageBody>(body);
                _logger.LogDebug("Successfully deserialized FollowUpMessageBody");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deserialize FollowUpMessageBody. Returning an explicit null");
                return null;
            }

            return followUpMessageBody;
        }

        public NetworkingMessageBody DeserializeNetworkingMessageBody(string body)
        {
            _logger.LogInformation("Deserializing NetworkingMessageBody");
            NetworkingMessageBody message = null;
            try
            {
                message = JsonConvert.DeserializeObject<NetworkingMessageBody>(body);
                _logger.LogDebug("Successfully deserialized NetworkingMessageBody");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deserialize NetworkingMessageBody. Returning an explicit null");
                return null;
            }

            return message;
        }

        public MonitorForNewAcceptedConnectionsBody DeserializeMonitorForNewAcceptedConnectionsBody(string body)
        {
            _logger.LogInformation("Deserializing MonitorForNewAcceptedConnectionsBody");
            MonitorForNewAcceptedConnectionsBody message = null;
            try
            {
                message = JsonConvert.DeserializeObject<MonitorForNewAcceptedConnectionsBody>(body);
                _logger.LogDebug("Successfully deserialized MonitorForNewAcceptedConnectionsBody");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deserialize MonitorForNewAcceptedConnectionsBody. Returning an explicit null");
                return null;
            }

            return message;
        }

        public ProspectListBody DeserializeProspectListBody(string body)
        {
            _logger.LogInformation("Deserializing ProspectListBody");
            ProspectListBody prospectListBody = null;
            try
            {
                prospectListBody = JsonConvert.DeserializeObject<ProspectListBody>(body);
                _logger.LogDebug("Successfully deserialized ProspectListBody");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deserialize ProspectListBody. Returning an explicit null");
                return null;
            }
            return prospectListBody;
        }

        public ScanProspectsForRepliesBody DeserializeScanProspectsForRepliesBody(string body)
        {
            _logger.LogInformation("Deserializing ScanProspectsForRepliesBody");
            ScanProspectsForRepliesBody scanProspectsForRepliesBody = null;
            try
            {
                scanProspectsForRepliesBody = JsonConvert.DeserializeObject<ScanProspectsForRepliesBody>(body);
                _logger.LogDebug("Successfully deserialized ScanProspectsForRepliesBody");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deserialize ScanProspectsForRepliesBody. Returning an explicit null");
                return null;
            }

            return scanProspectsForRepliesBody;
        }

        public SendConnectionsBody DeserializeSendConnectionRequestsBody(string body)
        {
            _logger.LogInformation("Deserializing SendConnectionsBody");
            SendConnectionsBody sendConnectionsBody = null;
            try
            {
                sendConnectionsBody = JsonConvert.DeserializeObject<SendConnectionsBody>(body);
                _logger.LogDebug("Successfully deserialized SendConnectionsBody");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deserialize SendConnectionsBody. Returning an explicit null");
                return null;
            }

            return sendConnectionsBody;
        }
    }
}
