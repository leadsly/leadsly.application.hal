using Domain.Facades.Interfaces;
using Domain.Providers.Campaigns;
using Domain.Serializers.Interfaces;
using Hangfire;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.Responses;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.PhaseHandlers.MonitorForNewConnectionsHandler
{
    public class MonitorForNewConnectionsCommandHandler : ICommandHandler<MonitorForNewConnectionsCommand>
    {
        public MonitorForNewConnectionsCommandHandler(
            ILogger<MonitorForNewConnectionsCommandHandler> logger,
            ICampaignPhaseFacade campaignPhaseFacade
            )
        {
            _campaignPhaseFacade = campaignPhaseFacade;
            _logger = logger;
        }

        private readonly ILogger<MonitorForNewConnectionsCommandHandler> _logger;
        private readonly ICampaignPhaseFacade _campaignPhaseFacade;        

        public async Task HandleAsync(MonitorForNewConnectionsCommand command)
        {
            IModel channel = command.Channel;
            BasicDeliverEventArgs eventArgs = command.EventArgs;
            channel.BasicAck(eventArgs.DeliveryTag, false);

            MonitorForNewAcceptedConnectionsBody body = command.MessageBody as MonitorForNewAcceptedConnectionsBody;

            if (MonitorForNewProspectsProvider.IsRunning == false)
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                // this is required because this task can run for 8 - 10 hours a day. The AppServer does not know IF this task/phase is already
                // running on Hal thus it will trigger messages blindly. Otherwise if we await this here, then none of the blindly triggered
                // messages make it here, thus clugg up the queue
                Task cacheTask = Task.Run(() =>
                {
                    StartMonitorForNewConnectionsAsync(command, body);
                });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            }
        }

        public async Task StartMonitorForNewConnectionsAsync(MonitorForNewConnectionsCommand command, MonitorForNewAcceptedConnectionsBody monitorForNewAcceptedConnections)
        {
            HalOperationResult<IOperationResponse> operationResult = await _campaignPhaseFacade.ExecutePhase<IOperationResponse>(monitorForNewAcceptedConnections);

            if (operationResult.Succeeded == true)
            {
                _logger.LogInformation("ExecuteFollowUpMessagesPhase executed successfully. Acknowledging message");
            }
            else
            {
                _logger.LogWarning("Executing Follow Up Messages Phase did not successfully succeeded. Negatively acknowledging the message and re-queuing it");
            }            
        }
    }
}
