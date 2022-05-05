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

namespace Domain.PhaseHandlers.ScanProspectsForRepliesHandler
{
    public class DeepScanProspectsForRepliesCommandHandler : ICommandHandler<DeepScanProspectsForRepliesCommand>
    {
        public DeepScanProspectsForRepliesCommandHandler(
            ILogger<DeepScanProspectsForRepliesCommandHandler> logger,
            ICampaignPhaseFacade campaignPhaseFacade
            )
        {
            _campaignPhaseFacade = campaignPhaseFacade;            
            _logger = logger;
        }

        private readonly ILogger<DeepScanProspectsForRepliesCommandHandler> _logger;
        private readonly ICampaignPhaseFacade _campaignPhaseFacade;        

        public async Task HandleAsync(DeepScanProspectsForRepliesCommand command)
        {
            IModel channel = command.Channel;
            BasicDeliverEventArgs eventArgs = command.EventArgs;

            ScanProspectsForRepliesBody body = command.MessageBody as ScanProspectsForRepliesBody;

            try
            {
                await StartDeepScanningProspectsForRepliesAsync(body);
                channel.BasicAck(eventArgs.DeliveryTag, false);
                _logger.LogInformation("Successfully acknowledged DeepScanProspectsForReplies phase");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error occured execution ScanProspectsForReplies 'ExecuteOnce' phase. Requeing message");
                channel.BasicNack(eventArgs.DeliveryTag, false, true);
            }
        }

        private async Task StartDeepScanningProspectsForRepliesAsync(ScanProspectsForRepliesBody scanProspectsForRepliesBody)
        {
            try
            {
                HalOperationResult<IOperationResponse>  operationResult = await _campaignPhaseFacade.ExecuteDeepScanPhaseAsync<IOperationResponse>(scanProspectsForRepliesBody);
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
