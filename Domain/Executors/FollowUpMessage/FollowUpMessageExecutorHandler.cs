using Domain.Models.Requests;
using Domain.Orchestrators.Interfaces;
using Domain.Services.Interfaces;
using Leadsly.Application.Model.Campaigns;
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
            ISendFollowUpMessageService service
            )
        {
            _logger = logger;
            _service = service;
            _orchestrator = orchestrator;
        }

        private readonly ILogger<FollowUpMessageExecutorHandler> _logger;
        private readonly ISendFollowUpMessageService _service;
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
            SentFollowUpMessageRequest request = _orchestrator.GetSentFollowUpMessage();
            if (request != null)
            {
                await _service.ProcessSentFollowUpMessageAsync(request, message);
            }
        }
    }
}
