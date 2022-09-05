using Domain.Models.FollowUpMessage;
using Domain.Models.RabbitMQMessages;
using Domain.Orchestrators.Interfaces;
using Domain.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Domain.Executors.FollowUpMessage
{
    public class FollowUpMessageExecutorHandler : IMessageExecutorHandler<FollowUpMessageBody>
    {
        public FollowUpMessageExecutorHandler(
            ILogger<FollowUpMessageExecutorHandler> logger,
            IFollowUpMessagePhaseOrchestrator orchestrator,
            IFollowUpMessageService service
            )
        {
            _logger = logger;
            _service = service;
            _orchestrator = orchestrator;
        }

        private readonly ILogger<FollowUpMessageExecutorHandler> _logger;
        private readonly IFollowUpMessageService _service;
        private readonly IFollowUpMessagePhaseOrchestrator _orchestrator;

        public async Task<bool> ExecuteMessageAsync(FollowUpMessageBody message)
        {
            bool succeeded = false;
            try
            {
                _orchestrator.Execute(message);
                succeeded = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured executing FollowUpMessage phase");
                succeeded = false;
            }
            finally
            {
                await ProcessSentFollowUpMessageAsync(message);
            }

            return succeeded;
        }

        private async Task ProcessSentFollowUpMessageAsync(FollowUpMessageBody message)
        {
            SentFollowUpMessage item = _orchestrator.GetSentFollowUpMessage();
            if (item != null)
            {
                await _service.ProcessSentFollowUpMessageAsync(item, message);
            }
        }
    }
}
