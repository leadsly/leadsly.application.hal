using Leadsly.Application.Model.Campaigns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Serializers.Interfaces
{
    public interface ICampaignPhaseSerializer
    {
        FollowUpMessagesBody DeserializeFollowUpMessagesBody(string body);

        ScanProspectsForRepliesBody DeserializeScanProspectsForRepliesBody(string body);

        MonitorForNewAcceptedConnectionsBody DeserializeMonitorForNewAcceptedConnectionsBody(string body);

        ProspectListBody DeserializeProspectListBody(string body);

        SendConnectionsBody DeserializeSendConnectionRequestsBody(string body);

        ConnectionWithdrawBody DeserializeConnectionWithdrawBody(string body);

    }
}
