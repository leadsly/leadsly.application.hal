using Domain.Models.RabbitMQMessages;
using Domain.Models.Responses;
using Domain.Orchestrators.Interfaces;
using Domain.RabbitMQ.Interfaces;
using Domain.Services.Interfaces;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Executors.ScanProspectsForReplies
{
    public class DeepScanProspectsForRepliesExecutorHandler : IMessageExecutorHandler<DeepScanProspectsForRepliesBody>
    {
        public DeepScanProspectsForRepliesExecutorHandler(
            ILogger<DeepScanProspectsForRepliesExecutorHandler> logger,
            IDeepScanProspectsForRepliesPhaseOrchestrator orchestrator,
            IDeepScanProspectsService service,
            IRabbitMQManager rabbitMQ
            )
        {
            _logger = logger;
            _orchestrator = orchestrator;
            _service = service;
            _rabbitMQ = rabbitMQ;
        }

        private readonly IRabbitMQManager _rabbitMQ;
        private readonly IDeepScanProspectsService _service;
        private readonly ILogger<DeepScanProspectsForRepliesExecutorHandler> _logger;
        private readonly IDeepScanProspectsForRepliesPhaseOrchestrator _orchestrator;

        public async Task<bool> ExecuteMessageAsync(DeepScanProspectsForRepliesBody message)
        {
            try
            {
                NetworkProspectsResponse networkProspects = await _service.GetAllProspectsFromActiveCampaignsAsync(message);
                if (networkProspects == null || networkProspects.Items.Count == 0)
                {
                    _logger.LogDebug("No network prospects were retrieved. DeepScanProspectsPhase will not be triggered");
                }
                else
                {
                    _orchestrator.Execute(message, networkProspects.Items);
                }
            }
            finally
            {
                await ProcessProspectsThatRepliedAsync(message);
                PublishTriggerScanProspectsForReplies(message);
                PublishTriggerSendFollowUpMessages(message);
            }

            return true;
        }

        private async Task ProcessProspectsThatRepliedAsync(DeepScanProspectsForRepliesBody message)
        {
            if (_orchestrator.Prospects.Count > 0)
            {
                await _service.ProcessCampaignProspectsThatRepliedAsync(_orchestrator.Prospects, message);
            }
        }

        private void PublishTriggerScanProspectsForReplies(DeepScanProspectsForRepliesBody message)
        {
            try
            {
                // publish scan prospects for replies phase here
                TriggerScanProspectsForRepliesMessage messageBody = new()
                {
                    HalId = message.HalId,
                    UserId = message.UserId
                };
                string msg = JsonConvert.SerializeObject(messageBody);
                byte[] rawMessage = Encoding.UTF8.GetBytes(msg);
                _rabbitMQ.PublishMessage(rawMessage, RabbitMQConstants.TriggerScanProspectsForReplies.QueueName, RabbitMQConstants.TriggerScanProspectsForReplies.RoutingKey);
            }
            catch (Exception ex)
            {
                string halId = message.HalId;
                _logger.LogError(ex, "Failed to publish TriggerScanProspectsForReplies message for halId {halId}", halId);
            }
        }

        private void PublishTriggerSendFollowUpMessages(DeepScanProspectsForRepliesBody message)
        {
            try
            {
                // publish trigger follow up messages phase here
                TriggerSendFollowUpMessages messageBody = new()
                {
                    HalId = message.HalId,
                    UserId = message.UserId
                };
                string msg = JsonConvert.SerializeObject(messageBody);
                byte[] rawMessage = Encoding.UTF8.GetBytes(msg);
                _rabbitMQ.PublishMessage(rawMessage, RabbitMQConstants.TriggerFollowUpMessages.QueueName, RabbitMQConstants.TriggerFollowUpMessages.RoutingKey);
            }
            catch (Exception ex)
            {
                string halId = message.HalId;
                _logger.LogError(ex, "Failed to publish TriggerSendFollowUpMessages message for halId {halId}", halId);
            }
        }
    }
}
