using Domain.Executors.AllInOneVirtualAssistant.Events;
using Domain.Executors.MonitorForNewConnections.Events;
using Domain.Executors.ScanProspectsForReplies.Events;
using Domain.Models.FollowUpMessage;
using Domain.Models.Networking;
using Domain.Models.ProspectList;
using Domain.Models.Responses;
using Domain.Models.SendConnections;
using Domain.MQ.Interfaces;
using Domain.MQ.Messages;
using Domain.Orchestrators.Interfaces;
using Domain.Services.Interfaces;
using Leadsly.Application.Model;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Executors.AllInOneVirtualAssistant
{
    public class AllInOneVirtualAssistantMessageExecutorHandler : IMessageExecutorHandler<AllInOneVirtualAssistantMessageBody>
    {
        public AllInOneVirtualAssistantMessageExecutorHandler(
            ILogger<AllInOneVirtualAssistantMessageExecutorHandler> logger,
            IRabbitMQManager rabbitMQ,
            IAllInOneVirtualAssistantService service,
            IAllInOneVirtualAssistantPhaseOrchestrator orchestrator)
        {
            _rabbitMQ = rabbitMQ;
            _service = service;
            _logger = logger;
            _orchestrator = orchestrator;
        }

        private readonly ILogger<AllInOneVirtualAssistantMessageExecutorHandler> _logger;
        private readonly IAllInOneVirtualAssistantPhaseOrchestrator _orchestrator;
        private readonly IRabbitMQManager _rabbitMQ;
        private readonly IAllInOneVirtualAssistantService _service;

        public async Task<bool> ExecuteMessageAsync(AllInOneVirtualAssistantMessageBody message)
        {
            bool succeeded = false;
            try
            {
                // wire up the event handler
                _orchestrator.NewConnectionsDetected += OnNewConnectionsDetected;
                _orchestrator.NewMessagesReceived += OnNewMessagesReceived;
                _orchestrator.UpdateRecentlyAddedProspects += OnUpdateConnectedProspectsReceived;

                // fetch search progress urls
                await GetSearchUrlsProgressAsync(message);

                // fetch previous connected with prospects, this list should include the total connections count, as well as a list of
                // prospects first name last name subheading and when we connected with them
                ConnectedNetworkProspectsResponse previousMonitoredResponse = await _service.GetAllPreviouslyConnectedNetworkProspectsAsync(message);
                if (previousMonitoredResponse == null)
                {
                    _logger.LogError("Error occured executing {0}. An error occured retrieving data. The response was null. HalId {1}", nameof(AllInOneVirtualAssistantMessageBody), message.HalId);
                }
                else
                {
                    message.PreviousMonitoredResponse = previousMonitoredResponse;
                    _orchestrator.Execute(message);
                }

                succeeded = true;
            }
            finally
            {
                await ProcessDataAsync(message);
                PublishDeprovisionResources(message);
            }

            return succeeded;
        }

        private async Task GetSearchUrlsProgressAsync(AllInOneVirtualAssistantMessageBody message)
        {
            Queue<NetworkingMessageBody> networkingMessages = new Queue<NetworkingMessageBody>();

            ParallelOptions parallelOptions = new()
            {
                MaxDegreeOfParallelism = 3
            };
            if (message.NetworkingMessages?.Count > 0)
            {
                await Parallel.ForEachAsync(message.NetworkingMessages, parallelOptions, async (networkingMessage, ct) =>
                {
                    GetSearchUrlProgressResponse response = await _service.GetSearchUrlProgressAsync(networkingMessage);
                    if (response != null && response.SearchUrls != null && response.SearchUrls.Count > 0)
                    {
                        networkingMessage.SearchUrlsProgress = response.SearchUrls;
                        networkingMessages.Enqueue(networkingMessage);
                    }
                });
            }

            message.NetworkingMessages = networkingMessages;
        }

        private async Task ProcessDataAsync(AllInOneVirtualAssistantMessageBody message)
        {
            // perform all async calls now.
            IList<ConnectionSentModel> connectionSents = _orchestrator.ConnectionsSent;
            if (connectionSents?.Count > 0)
            {
                await _service.ProcessSentConnectionsAsync(connectionSents, message);
            }

            IList<SearchUrlProgressModel> searchUrlsProgress = _orchestrator.GetUpdatedSearchUrlsProgress();
            if (searchUrlsProgress?.Count > 0)
            {
                await _service.UpdateSearchUrlsAsync(searchUrlsProgress, message);
            }

            List<PersistPrimaryProspectModel> persistPrimaryProspects = _orchestrator.PersistPrimaryProspects;
            if (persistPrimaryProspects?.Count > 0)
            {
                await _service.ProcessProspectListAsync(persistPrimaryProspects, message);
            }

            bool monthlyLimitReached = _orchestrator.GetMonthlySearchLimitReached();
            await _service.UpdateMonthlySearchLimitAsync(monthlyLimitReached, message);

            IList<SentFollowUpMessageModel> sentFollowUpMessages = _orchestrator.GetSentFollowUpMessages();
            ParallelOptions parallelOptions = new()
            {
                MaxDegreeOfParallelism = 3
            };
            if (sentFollowUpMessages?.Count > 0)
            {
                await Parallel.ForEachAsync(sentFollowUpMessages, parallelOptions, async (sentFollowUpMessage, ct) =>
                {
                    await _service.ProcessSentFollowUpMessageAsync(sentFollowUpMessage, message);
                });
            }
        }

        private void PublishDeprovisionResources(AllInOneVirtualAssistantMessageBody message)
        {
            try
            {
                DeprovisionResourcesBody messageBody = new()
                {
                    HalId = message.HalId,
                    UserId = message.UserId
                };
                string msg = JsonConvert.SerializeObject(messageBody);
                byte[] rawMessage = Encoding.UTF8.GetBytes(msg);
                _rabbitMQ.PublishMessage(rawMessage, RabbitMQConstants.DeprovisionResources.QueueName, RabbitMQConstants.DeprovisionResources.RoutingKey);
            }
            catch (Exception ex)
            {
                string halId = message.HalId;
                _logger.LogError(ex, "Failed to publish {0} message for halId {1}", nameof(DeprovisionResourcesBody), halId);
            }
        }

        private async Task OnNewMessagesReceived(object sender, NewMessagesReceivedEventArgs e)
        {
            _logger.LogDebug("New messages received. Sending them to the server for processing");
            await _service.ProcessNewMessagesAsync(e.NewMessages, e.Message);
        }

        private async Task OnNewConnectionsDetected(object sender, NewRecentlyAddedProspectsDetectedEventArgs e)
        {
            _logger.LogDebug("Executing {0}. New prospects have been detected. Sending them to the server for processing. HalId {1}", nameof(AllInOneVirtualAssistantMessageBody), e.Message.HalId);
            await _service.ProcessRecentlyAddedProspectsAsync(e.NewRecentlyAddedProspects, e.Message);
        }

        private async Task OnUpdateConnectedProspectsReceived(object sender, UpdateRecentlyAddedProspectsEventArgs e)
        {
            _logger.LogInformation("Executing {0}. Preparing request to update previously connected network prospects. HalId {1}", nameof(AllInOneVirtualAssistantMessageBody), e.Message.HalId);
            await _service.UpdatePreviouslyConnectedNetworkProspectsAsync(e.Message, e.NewRecentlyAddedProspects, e.TotalConnectionsCount);
        }
    }
}
