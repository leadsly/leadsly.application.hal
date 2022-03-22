using Domain.Serializers.Interfaces;
using Leadsly.Application.Model.Campaigns;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Serializers
{
    public class CampaignPhaseSerializer : ICampaignPhaseSerializer
    {
        public CampaignPhaseSerializer()
        {

        }

        public ConnectionWithdrawBody DeserializeConnectionWithdrawBody(string body)
        {
            throw new NotImplementedException();
        }

        public FollowUpMessagesBody DeserializeFollowUpMessagesBody(string body)
        {
            FollowUpMessagesBody followUpMessageBody = null;
            try
            {
                followUpMessageBody = JsonConvert.DeserializeObject<FollowUpMessagesBody>(body);
            }
            catch(Exception ex)
            {

            }

            return followUpMessageBody;
        }

        public MonitorForNewAcceptedConnectionsBody DeserializeMonitorForNewAcceptedConnectionsBody(string body)
        {
            MonitorForNewAcceptedConnectionsBody message = null;
            try
            {
                message = JsonConvert.DeserializeObject<MonitorForNewAcceptedConnectionsBody>(body);
            }
            catch (Exception ex)
            {

            }

            return message;
        }

        public ProspectListBody DeserializeProspectListBody(string body)
        {
            throw new NotImplementedException();
        }

        public ScanProspectsForRepliesBody DeserializeScanProspectsForRepliesBody(string body)
        {
            ScanProspectsForRepliesBody scanProspectsForRepliesBody = null;
            try
            {
                scanProspectsForRepliesBody = JsonConvert.DeserializeObject<ScanProspectsForRepliesBody>(body);
            }
            catch (Exception ex)
            {

            }

            return scanProspectsForRepliesBody;
        }

        public SendConnectionRequestsBody DeserializeSendConnectionRequestsBody(string body)
        {
            throw new NotImplementedException();
        }
    }
}
