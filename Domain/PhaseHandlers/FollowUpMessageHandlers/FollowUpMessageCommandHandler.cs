using Domain.Facades.Interfaces;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.Responses;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Threading.Tasks;

namespace Domain.PhaseHandlers.FollowUpMessageHandlers
{
    public class FollowUpMessageCommandHandler : ICommandHandler<FollowUpMessageCommand>
    {
        public FollowUpMessageCommandHandler(
            ICampaignPhaseFacade campaignPhaseFacade,
            ILogger<FollowUpMessageCommandHandler> logger)
        {
            _campaignPhaseFacade = campaignPhaseFacade;
            _logger = logger;
        }

        private readonly ILogger<FollowUpMessageCommandHandler> _logger;
        private readonly ICampaignPhaseFacade _campaignPhaseFacade;

        public async Task HandleAsync(FollowUpMessageCommand command)
        {
            IModel channel = command.Channel;
            BasicDeliverEventArgs args = command.EventArgs;

            FollowUpMessageBody body = command.MessageBody as FollowUpMessageBody;

            await StartFollowUpMessagesAsync(body);
            channel.BasicAck(args.DeliveryTag, false);
        }

        private async Task StartFollowUpMessagesAsync(FollowUpMessageBody followUpMessages)
        {
            HalOperationResult<IOperationResponse> operationResult = await _campaignPhaseFacade.ExecutePhaseAsync<IOperationResponse>(followUpMessages);

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
