using Domain.Executors;
using Domain.Facades.Interfaces;
using Leadsly.Application.Model.Campaigns;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Threading.Tasks;

namespace Domain.PhaseHandlers.ScanProspectsForRepliesHandler
{
    public class ScanProspectsForRepliesCommandHandler : ICommandHandler<ScanProspectsForRepliesCommand>
    {
        public ScanProspectsForRepliesCommandHandler(
            ILogger<ScanProspectsForRepliesCommandHandler> logger,
            IMessageExecutorHandler<ScanProspectsForRepliesBody> messageExecutorHandler,
            ICampaignPhaseFacade campaignPhaseFacade)
        {
            _logger = logger;
            _campaignPhaseFacade = campaignPhaseFacade;
            _messageExecutorHandler = messageExecutorHandler;
        }

        private readonly IMessageExecutorHandler<ScanProspectsForRepliesBody> _messageExecutorHandler;
        private readonly ILogger<ScanProspectsForRepliesCommandHandler> _logger;
        private readonly ICampaignPhaseFacade _campaignPhaseFacade;

        public async Task HandleAsync(ScanProspectsForRepliesCommand command)
        {
            IModel channel = command.Channel;
            BasicDeliverEventArgs eventArgs = command.EventArgs;

            // acknowledge the message right away. Let hangfire handle retryies
            channel.BasicAck(eventArgs.DeliveryTag, false);

            ScanProspectsForRepliesBody message = command.MessageBody as ScanProspectsForRepliesBody;

            bool succeeded = await _messageExecutorHandler.ExecuteMessageAsync(message);

            //            if (ScanProspectsForRepliesProvider.IsRunning == false)
            //            {
            //                _logger.LogInformation("ScanProspectsForReplies phase is currently NOT running. Executing the phase until the end of work day");
            //#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            //                // this is required because this task can run for 8 - 10 hours a day. The AppServer does not know IF this task/phase is already
            //                // running on Hal thus it will trigger messages blindly. Otherwise if we await this here, then none of the blindly triggered
            //                // messages make it here, thus clugg up the queue
            //                Task.Run(() =>
            //                {
            //                    StartScanningProspectsForRepliesAsync(command, message);
            //                });
            //#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            //            }
            //            else
            //            {
            //                _logger.LogInformation("ScanProspectsForReplies phase is currently running.");
            //            }
        }

        //public async Task StartScanningProspectsForRepliesAsync(ScanProspectsForRepliesCommand command, ScanProspectsForRepliesBody body)
        //{
        //    HalOperationResult<IOperationResponse> operationResult = await _campaignPhaseFacade.ExecutePhaseAsync<IOperationResponse>(body);
        //    if (operationResult.Succeeded == true)
        //    {
        //        _logger.LogInformation("ExecuteScanProspectsForRepliesPhase executed successfully. Acknowledging message");
        //    }
        //    else
        //    {
        //        _logger.LogWarning("ExecuteScanProspectsForRepliesPhase did not successfully succeeded. Negatively acknowledging the message and re-queuing it");
        //    }
        //}

    }
}
