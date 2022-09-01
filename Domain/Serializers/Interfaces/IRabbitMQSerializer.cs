using Leadsly.Application.Model.Campaigns;

namespace Domain.Serializers.Interfaces
{
    public interface IRabbitMQSerializer
    {
        FollowUpMessageBody DeserializeFollowUpMessagesBody(string body);

        ScanProspectsForRepliesBody DeserializeScanProspectsForRepliesBody(string body);
        DeepScanProspectsForRepliesBody DeserializeDeepScanProspectsForRepliesBody(string body);

        MonitorForNewAcceptedConnectionsBody DeserializeMonitorForNewAcceptedConnectionsBody(string body);

        NetworkingMessageBody DeserializeNetworkingMessageBody(string body);

        ProspectListBody DeserializeProspectListBody(string body);

        SendConnectionsBody DeserializeSendConnectionRequestsBody(string body);

        ConnectionWithdrawBody DeserializeConnectionWithdrawBody(string body);
    }
}
