using Domain.Facades.Interfaces;
using Domain.Serializers.Interfaces;
using Hangfire;
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

namespace Domain.PhaseHandlers.ScanProspectsForRepliesHandler
{
    public class ScanProspectsForRepliesCommandHandler : ICommandHandler<ScanProspectsForRepliesCommand>
    {
        public ScanProspectsForRepliesCommandHandler(
            ILogger<ScanProspectsForRepliesCommandHandler> logger,
            ICampaignPhaseFacade campaignPhaseFacade,
            IRabbitMQSerializer serializer)
        {
            _campaignPhaseFacade = campaignPhaseFacade;
            _serializer = serializer;
        }

        private readonly ILogger<ScanProspectsForRepliesCommandHandler> _logger;
        private readonly ICampaignPhaseFacade _campaignPhaseFacade;
        private readonly IRabbitMQSerializer _serializer;

        public async Task HandleAsync(ScanProspectsForRepliesCommand command)
        {
            IModel channel = command.Channel;
            BasicDeliverEventArgs eventArgs = command.EventArgs;

            // acknowledge the message right away. Let hangfire handle retryies
            // channel.BasicAck(eventArgs.DeliveryTag, false);

            byte[] body = eventArgs.Body.ToArray();
            string message = Encoding.UTF8.GetString(body);
            ScanProspectsForRepliesBody scanProspectsForRepliesBody = _serializer.DeserializeScanProspectsForRepliesBody(message);

            try
            {
                BackgroundJob.Enqueue(() => StartScanningProspectsForRepliesAsync(scanProspectsForRepliesBody));                
            }
            catch (Exception ex)
            {
                _logger.LogError("Error occured execution ScanProspectsForReplies 'ExecuteOnce' phase. Requeing message");
                channel.BasicNack(eventArgs.DeliveryTag, false, true);
            }
        }

        [AutomaticRetry(Attempts = 5)]
        private async Task StartScanningProspectsForRepliesAsync(ScanProspectsForRepliesBody scanProspectsForRepliesBody)
        {
            try
            {
                HalOperationResult<IOperationResponse> operationResult = await _campaignPhaseFacade.ExecutePhaseAsync<IOperationResponse>(scanProspectsForRepliesBody);
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
                throw;
            }
        }

    }
}
