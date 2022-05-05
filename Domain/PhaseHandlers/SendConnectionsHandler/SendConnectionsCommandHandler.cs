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

namespace Domain.PhaseHandlers.SendConnectionsHandler
{
    public class SendConnectionsCommandHandler : ICommandHandler<SendConnectionsCommand>
    {
        public SendConnectionsCommandHandler(
            ILogger<SendConnectionsCommandHandler> logger,
            ICampaignPhaseFacade campaignPhaseFacade
            )
        {
            _campaignPhaseFacade = campaignPhaseFacade;
            _logger = logger;            
        }

        private readonly ILogger<SendConnectionsCommandHandler> _logger;
        private readonly ICampaignPhaseFacade _campaignPhaseFacade;        

        public async Task HandleAsync(SendConnectionsCommand command)
        {
            IModel channel = command.Channel;
            BasicDeliverEventArgs eventArgs = command.EventArgs;

            SendConnectionsBody body = command.MessageBody as SendConnectionsBody;

            Action ackOperation = default;
            try
            {
                HalOperationResult<IOperationResponse> operationResult = await _campaignPhaseFacade.ExecutePhaseAsync<IOperationResponse>(body);

                if (operationResult.Succeeded == true)
                {
                    _logger.LogInformation("SendConnectionRequests executed successfully. Acknowledging message");
                    ackOperation = () => channel.BasicAck(eventArgs.DeliveryTag, false);
                }
                else
                {
                    _logger.LogWarning("SendConnectionRequests phase did not successfully execute. Negatively acknowledging the message and re-queuing it");
                    ackOperation = () => channel.BasicNack(eventArgs.DeliveryTag, false, true);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occured while executing send connection requests. Negatively acknowledging the message and re-queuing it");
                ackOperation = () => channel.BasicNack(eventArgs.DeliveryTag, false, true);
            }
            finally
            {
                ackOperation();
            }

        }
    }
}
