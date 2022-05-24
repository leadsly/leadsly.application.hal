using Leadsly.Application.Model.Campaigns;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Services.Interfaces
{
    public interface IPhaseEventHandlerService
    {
        Task OnNetworkingConnectionsEventReceivedAsync(object sender, BasicDeliverEventArgs eventArgs);
        Task OnFollowUpMessageEventReceivedAsync(object sender, BasicDeliverEventArgs eventArgs);        
        Task OnMonitorForNewAcceptedConnectionsEventReceivedAsync(object sender, BasicDeliverEventArgs eventArgs);
        Task OnNetworkingEventReceivedAsync(object sender, BasicDeliverEventArgs eventArgs);
        Task OnScanProspectsForRepliesEventReceivedAsync(object sender, BasicDeliverEventArgs eventArgs);
        Task OnConnectionWithdrawEventReceivedAsync(object sender, BasicDeliverEventArgs eventArgs);
    }
}
