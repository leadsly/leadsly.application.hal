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
