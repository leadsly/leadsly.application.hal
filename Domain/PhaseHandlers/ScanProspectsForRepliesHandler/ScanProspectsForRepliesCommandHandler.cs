using Domain.Facades.Interfaces;
using Domain.Providers.Campaigns;
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
    public class ScanProspectsForRepliesCommandHandler : ICommandHandler<ScanProspectsForRepliesCommand>
    {
        public ScanProspectsForRepliesCommandHandler(
            ILogger<ScanProspectsForRepliesCommandHandler> logger,
            ICampaignPhaseFacade campaignPhaseFacade)
        {
            _logger = logger;
            _campaignPhaseFacade = campaignPhaseFacade;
        }

        private readonly ILogger<ScanProspectsForRepliesCommandHandler> _logger;
        private readonly ICampaignPhaseFacade _campaignPhaseFacade;

        public async Task HandleAsync(ScanProspectsForRepliesCommand command)
        {
            IModel channel = command.Channel;
            BasicDeliverEventArgs eventArgs = command.EventArgs;

            // acknowledge the message right away. Let hangfire handle retryies
            channel.BasicAck(eventArgs.DeliveryTag, false);

            ScanProspectsForRepliesBody body = command.MessageBody as ScanProspectsForRepliesBody;

            if (ScanProspectsForRepliesProvider.IsRunning == false)
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                // this is required because this task can run for 8 - 10 hours a day. The AppServer does not know IF this task/phase is already
                // running on Hal thus it will trigger messages blindly. Otherwise if we await this here, then none of the blindly triggered
                // messages make it here, thus clugg up the queue
                Task.Run(() =>
                {
                    StartScanningProspectsForRepliesAsync(command, body);
                });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
        }

        public async Task StartScanningProspectsForRepliesAsync(ScanProspectsForRepliesCommand command, ScanProspectsForRepliesBody body)
        {
            try
            {
                HalOperationResult<IOperationResponse> operationResult = await _campaignPhaseFacade.ExecutePhaseAsync<IOperationResponse>(body);
                if (operationResult.Succeeded == true)
                {
                    _logger.LogInformation("ExecuteScanProspectsForRepliesPhase executed successfully. Acknowledging message");
                }
                else
                {
                    _logger.LogWarning("ExecuteScanProspectsForRepliesPhase did not successfully succeeded. Negatively acknowledging the message and re-queuing it");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occured while executing ExecuteScanProspectsForRepliesPhase");
                command.Channel.BasicNack(command.EventArgs.DeliveryTag, false, true);
            }
        }

    }
}
