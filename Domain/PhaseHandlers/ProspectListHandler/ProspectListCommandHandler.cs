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

namespace Domain.PhaseHandlers.NetworkingConnectionsHandler
{
    public class ProspectListCommandHandler : ICommandHandler<ProspectListCommand>
    {
        public ProspectListCommandHandler(
            ILogger<ProspectListCommandHandler> logger,
            ICampaignPhaseFacade campaignPhaseFacade, 
            IRabbitMQSerializer serializer)
        {
            _campaignPhaseFacade = campaignPhaseFacade;
            _serializer = serializer;
        }

        private readonly ILogger<ProspectListCommandHandler> _logger;
        private readonly ICampaignPhaseFacade _campaignPhaseFacade;
        private readonly IRabbitMQSerializer _serializer;

        public async Task HandleAsync(ProspectListCommand command)
        {
            IModel channel = command.Channel;
            BasicDeliverEventArgs eventArgs = command.EventArgs;

            byte[] body = eventArgs.Body.ToArray();
            string message = Encoding.UTF8.GetString(body);
            ProspectListBody prospectListBody = _serializer.DeserializeProspectListBody(message);
            Action ackOperation = default;
            try
            {
                HalOperationResult<IOperationResponse> operationResult = await _campaignPhaseFacade.ExecutePhaseAsync<IOperationResponse>(prospectListBody);

                if (operationResult.Succeeded == true)
                {
                    _logger.LogInformation("ExecuteFollowUpMessagesPhase executed successfully. Acknowledging message");
                    ackOperation = () => channel.BasicAck(eventArgs.DeliveryTag, false);
                }
                else
                {
                    _logger.LogWarning("Executing Follow Up Messages Phase did not successfully succeeded. Negatively acknowledging the message and re-queuing it");
                    ackOperation = () => channel.BasicNack(eventArgs.DeliveryTag, false, true);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occured while executing Follow Up Messages Phase. Negatively acknowledging the message and re-queuing it");
                ackOperation = () => channel.BasicNack(eventArgs.DeliveryTag, false, true);
            }
            finally
            {
                ackOperation();
            }
        }
    }
}
