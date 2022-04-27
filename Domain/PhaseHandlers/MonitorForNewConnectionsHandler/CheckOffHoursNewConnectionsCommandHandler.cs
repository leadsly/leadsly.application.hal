using Domain.Facades.Interfaces;
using Domain.Serializers.Interfaces;
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
    public class CheckOffHoursNewConnectionsCommandHandler : ICommandHandler<CheckOffHoursNewConnectionsCommand>
    {
        public CheckOffHoursNewConnectionsCommandHandler(
            ILogger<CheckOffHoursNewConnectionsCommandHandler> logger,
            ICampaignPhaseFacade campaignPhaseFacade,
            IRabbitMQSerializer serializer)
        {
            _campaignPhaseFacade = campaignPhaseFacade;
            _serializer = serializer;
        }

        private readonly ILogger<CheckOffHoursNewConnectionsCommandHandler> _logger;
        private readonly ICampaignPhaseFacade _campaignPhaseFacade;
        private readonly IRabbitMQSerializer _serializer;

        public async Task HandleAsync(CheckOffHoursNewConnectionsCommand command)
        {
            IModel channel = command.Channel;
            BasicDeliverEventArgs eventArgs = command.EventArgs;

            byte[] body = eventArgs.Body.ToArray();
            string message = Encoding.UTF8.GetString(body);
            MonitorForNewAcceptedConnectionsBody monitorForNewConnectionsBody = _serializer.DeserializeMonitorForNewAcceptedConnectionsBody(message);

            HalOperationResult<IOperationResponse> operationResult = await _campaignPhaseFacade.ExecuteOffHoursScanPhaseAsync<IOperationResponse>(monitorForNewConnectionsBody);
            if(operationResult.Succeeded == true)
            {
                channel.BasicAck(eventArgs.DeliveryTag, false);
            }
            else
            {
                channel.BasicNack(eventArgs.DeliveryTag, false, true);
            }            
        }
    }
}
