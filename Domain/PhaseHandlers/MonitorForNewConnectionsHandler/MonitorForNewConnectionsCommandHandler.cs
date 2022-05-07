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
            // channel.BasicAck(eventArgs.DeliveryTag, false);

            MonitorForNewAcceptedConnectionsBody body = command.MessageBody as MonitorForNewAcceptedConnectionsBody;

            if (MonitorForNewProspectsProvider.IsRunning == false)
            {
                await StartMonitorForNewConnectionsAsync(body);
            }            
        }

        public async Task StartMonitorForNewConnectionsAsync(MonitorForNewAcceptedConnectionsBody monitorForNewAcceptedConnections)
        {
            try
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occured while executing Follow Up Messages Phase. Negatively acknowledging the message and re-queuing it");
            }
        }
    }
}
