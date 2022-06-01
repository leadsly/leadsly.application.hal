using Domain.Facades.Interfaces;
using Domain.RabbitMQ;
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

namespace Domain.PhaseHandlers.ProspectListHandler
{
    public class ProspectListCommandHandler : ICommandHandler<ProspectListCommand>
    {
        public ProspectListCommandHandler(
            ILogger<ProspectListCommandHandler> logger,
            ICampaignPhaseFacade campaignPhaseFacade
            )
        {
            _campaignPhaseFacade = campaignPhaseFacade;
            _logger = logger;
        }

        private readonly ILogger<ProspectListCommandHandler> _logger;
        private readonly ICampaignPhaseFacade _campaignPhaseFacade;

        public async Task HandleAsync(ProspectListCommand command)
        {
            IModel channel = command.Channel;
            BasicDeliverEventArgs eventArgs = command.EventArgs;

            ProspectListBody body = command.MessageBody as ProspectListBody;
            
            try
            {
                HalOperationResult<IOperationResponse> operationResult = await _campaignPhaseFacade.ExecutePhaseAsync<IOperationResponse>(body);

                if (operationResult.Succeeded == true)
                {
                    _logger.LogInformation("ExecuteFollowUpMessagesPhase executed successfully. Acknowledging message");
                    channel.BasicAck(eventArgs.DeliveryTag, false);
                }
                else
                {
                    _logger.LogWarning("Executing Follow Up Messages Phase did not successfully succeeded. Negatively acknowledging the message and re-queuing it");
                    channel.BasicNackRetry(eventArgs);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occured while executing Follow Up Messages Phase. Negatively acknowledging the message and re-queuing it");
                channel.BasicNackRetry(eventArgs);
            }
        }
    }
}
