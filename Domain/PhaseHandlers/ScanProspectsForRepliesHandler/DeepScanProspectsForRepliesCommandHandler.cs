using Domain.Executors;
using Domain.Facades.Interfaces;
using Domain.RabbitMQ;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.Responses;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Threading.Tasks;

namespace Domain.PhaseHandlers.ScanProspectsForRepliesHandler
{
    public class DeepScanProspectsForRepliesCommandHandler : ICommandHandler<DeepScanProspectsForRepliesCommand>
    {
        public DeepScanProspectsForRepliesCommandHandler(
            ILogger<DeepScanProspectsForRepliesCommandHandler> logger,
            IMessageExecutorHandler<DeepScanProspectsForRepliesBody> messageExecutorHandler,
            ICampaignPhaseFacade campaignPhaseFacade
            )
        {
            _messageExecutorHandler = messageExecutorHandler;
            _campaignPhaseFacade = campaignPhaseFacade;
            _logger = logger;
        }

        private IMessageExecutorHandler<DeepScanProspectsForRepliesBody> _messageExecutorHandler;
        private readonly ILogger<DeepScanProspectsForRepliesCommandHandler> _logger;
        private readonly ICampaignPhaseFacade _campaignPhaseFacade;

        public async Task HandleAsync(DeepScanProspectsForRepliesCommand command)
        {
            IModel channel = command.Channel;
            BasicDeliverEventArgs eventArgs = command.EventArgs;

            DeepScanProspectsForRepliesBody message = command.MessageBody as DeepScanProspectsForRepliesBody;

            bool succeeded = await _messageExecutorHandler.ExecuteMessageAsync(message);

            try
            {
                ScanProspectsForRepliesBody body = command.MessageBody as ScanProspectsForRepliesBody;
                HalOperationResult<IOperationResponse> result = await StartDeepScanningProspectsForRepliesAsync(body);
                if (result.Succeeded == true)
                {
                    _logger.LogInformation("ExecuteScanProspectsForRepliesPhase executed successfully. Acknowledging message");
                    channel.BasicAck(eventArgs.DeliveryTag, false);
                }
                else
                {
                    _logger.LogWarning("ExecuteScanProspectsForRepliesPhase did not successfully succeeded. Negatively acknowledging the message and re-queuing it");
                    channel.BasicNackRetry(eventArgs);
                }
            }
            catch (Exception ex)
            {
                channel.BasicNackRetry(eventArgs);
            }

            _logger.LogInformation("Successfully acknowledged DeepScanProspectsForReplies phase");
        }

        private async Task<HalOperationResult<IOperationResponse>> StartDeepScanningProspectsForRepliesAsync(ScanProspectsForRepliesBody scanProspectsForRepliesBody)
        {
            HalOperationResult<IOperationResponse> operationResult = await _campaignPhaseFacade.ExecuteDeepScanPhaseAsync<IOperationResponse>(scanProspectsForRepliesBody);

            return operationResult;
        }
    }
}
