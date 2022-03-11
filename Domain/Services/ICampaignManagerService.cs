﻿using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Services
{
    public interface ICampaignManagerService
    {
        void OnFollowUpMessageEventReceived(object sender, BasicDeliverEventArgs eventArgs);
        
        void OnMonitorForNewAcceptedConnectionsEventReceived(object sender, BasicDeliverEventArgs eventArgs);

        void OnScanProspectsForRepliesEventReceived(object sender, BasicDeliverEventArgs eventArgs);

        void OnProspectListEventReceived(object sender, BasicDeliverEventArgs eventArgs);

        void OnSendConnectionRequestsEventReceived(object sender, BasicDeliverEventArgs eventArgs);

        void OnSendEmailInvitesEventReceived(object sender, BasicDeliverEventArgs eventArgs);

        void OnConnectionWithdrawEventReceived(object sender, BasicDeliverEventArgs eventArgs);

        void OnRescrapeSearchurlsEventReceived(object sender, BasicDeliverEventArgs eventArgs);
    }
}