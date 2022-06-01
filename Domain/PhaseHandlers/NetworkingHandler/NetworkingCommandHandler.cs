using Domain.Facades.Interfaces;
using Domain.RabbitMQ;
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

namespace Domain.PhaseHandlers.NetworkingHandler
{
    public class NetworkingCommandHandler : ICommandHandler<NetworkingCommand>
    {
        public NetworkingCommandHandler(
            ILogger<NetworkingCommandHandler> logger,
            ICampaignPhaseFacade campaignPhaseFacade
            )
        {
            _campaignPhaseFacade = campaignPhaseFacade;
            _logger = logger;
        }

        private readonly ILogger<NetworkingCommandHandler> _logger;
        private readonly ICampaignPhaseFacade _campaignPhaseFacade;

        public async Task HandleAsync(NetworkingCommand command)
        {
            IModel channel = command.Channel;
            BasicDeliverEventArgs eventArgs = command.EventArgs;

            NetworkingMessageBody body = command.MessageBody as NetworkingMessageBody;

            try
            {
                HalOperationResult<IOperationResponse> operationResult = await _campaignPhaseFacade.ExecutePhaseAsync<IOperationResponse>(body);
                if (operationResult.Succeeded == true)
                {
                    _logger.LogInformation("Networking phase executed successfully. Acknowledging message");
                    channel.BasicAck(eventArgs.DeliveryTag, false);
                }
                else
                {
                    _logger.LogWarning("Networking phase did not successfully execute. Negatively acknowledging the message and re-queuing it");
                    channel.BasicNackRetry(eventArgs);
                }
            }
            catch (Exception ex)
            {
                channel.BasicNackRetry(eventArgs);
            }
        }
    }
}
