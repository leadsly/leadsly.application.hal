using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Services
{
    public interface ICampaignManagerService
    {
        void NextPhase();
        void NegativeAcknowledge(IModel channel, ulong deliveryTag, bool retry);
        void PositiveAcknowledge(IModel channel, ulong deliveryTag);
        void ClearCurrentJob();

        void OnFollowUpMessageEventReceived(object channel, BasicDeliverEventArgs eventArgs);
        
        void OnMonitorForNewAcceptedConnectionsEventReceived(object channel, BasicDeliverEventArgs eventArgs);

        void OnScanProspectsForRepliesEventReceived(object channel, BasicDeliverEventArgs eventArgs);

        void OnProspectListEventReceived(object channel, BasicDeliverEventArgs eventArgs);

        void OnSendConnectionRequestsEventReceived(object channel, BasicDeliverEventArgs eventArgs);

        void OnSendEmailInvitesEventReceived(object channel, BasicDeliverEventArgs eventArgs);

        void OnConnectionWithdrawEventReceived(object channel, BasicDeliverEventArgs eventArgs);

        void OnRescrapeSearchurlsEventReceived(object channel, BasicDeliverEventArgs eventArgs);
    }
}
