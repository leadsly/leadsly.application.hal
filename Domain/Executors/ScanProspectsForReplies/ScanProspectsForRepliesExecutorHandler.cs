using Domain.Executors.ScanProspectsForReplies.Events;
using Domain.Orchestrators;
using Domain.Orchestrators.Interfaces;
using Domain.Services.Interfaces;
using Leadsly.Application.Model.Campaigns;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Domain.Executors.ScanProspectsForReplies
{
    public class ScanProspectsForRepliesExecutorHandler : IMessageExecutorHandler<ScanProspectsForRepliesBody>
    {
        public ScanProspectsForRepliesExecutorHandler(
            ILogger<ScanProspectsForRepliesExecutorHandler> logger,
            IScanProspectsForRepliesPhaseOrchestrator phaseOrchestrator,
            IScanProspectsService service
            )
        {
            _logger = logger;
            _phaseOrchestrator = phaseOrchestrator;
            _service = service;
        }

        private readonly IScanProspectsService _service;
        private readonly ILogger<ScanProspectsForRepliesExecutorHandler> _logger;
        private readonly IScanProspectsForRepliesPhaseOrchestrator _phaseOrchestrator;

        public Task<bool> ExecuteMessageAsync(ScanProspectsForRepliesBody message)
        {
            if (ScanProspectsForRepliesPhaseOrchestrator.IsRunning == false)
            {
                _logger.LogInformation("ScanProspectsForReplies phase is currently NOT running. Executing the phase until the end of work day");
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                // this is required because this task can run for 8 - 10 hours a day. The AppServer does not know IF this task/phase is already
                // running on Hal thus it will trigger messages blindly. Otherwise if we await this here, then none of the blindly triggered
                // messages make it here, thus clugg up the queue
                Task.Run(() =>
                {
                    _phaseOrchestrator.Execute(message);
                });

                _phaseOrchestrator.NewMessagesReceived += OnNewMessagesReceived;
                _phaseOrchestrator.EndOfWorkDayReached += OnEndOfWorkDayReached;
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }

            return Task.Run(() => true);
        }

        private void OnEndOfWorkDayReached(object sender, EndOfWorkDayReachedEventArgs e)
        {
            _logger.LogDebug("End of work day reached. Cleaning up resources");
            _phaseOrchestrator.EndOfWorkDayReached -= OnEndOfWorkDayReached;
            ScanProspectsForRepliesBody message = e.Message as ScanProspectsForRepliesBody;

            // publish messages to rabbitmq to clean up aws resouces at the end of work day
        }

        private async Task OnNewMessagesReceived(object sender, NewMessagesReceivedEventArgs e)
        {
            _logger.LogDebug("New messages received. Sending them to the server for processing");
            // do not unsubscribe from the event here because we need to respond to it each time it is received
            ScanProspectsForRepliesBody message = e.Message as ScanProspectsForRepliesBody;

            await _service.ProcessNewMessagesAsync(e.NewMessages, message);
        }
    }
}
