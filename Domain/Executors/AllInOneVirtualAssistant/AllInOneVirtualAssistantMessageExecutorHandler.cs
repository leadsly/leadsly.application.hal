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
            IAllInOneVirtualAssistantPhaseMetaOrchestrator orchestrator)
        {
            _rabbitMQ = rabbitMQ;
            _service = service;
            _logger = logger;
            _orchestrator = orchestrator;
        }

        private readonly ILogger<AllInOneVirtualAssistantMessageExecutorHandler> _logger;
        private readonly IAllInOneVirtualAssistantPhaseMetaOrchestrator _orchestrator;
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
                _orchestrator.FollowUpMessagesSent += OnFollowUpMessagesSent;

                // await SetupDeepScanProspectsForRepliesAsync(message);

                SetupCheckOffHoursConnections(message);

                // pull any networking messages
                await GetNetworkingSearchUrlsAsync(message);

                // pull any follow up messages
                // await GetFollowUpMessagesAsync(message);

                // fetch previous connected with prospects, this list should include the total connections count, as well as a list of
                // prospects first name last name subheading and when we connected with them
                ConnectedNetworkProspectsResponse previousMonitoredResponse = await _service.GetAllPreviouslyConnectedNetworkProspectsAsync(message);
                if (previousMonitoredResponse == null)
                {
                    _logger.LogError("Error occured executing {0}. An error occured retrieving data. The response was null. HalId {1}", nameof(AllInOneVirtualAssistantMessageBody), message.HalId);
                }

                message.PreviousMonitoredResponse = previousMonitoredResponse;
                _orchestrator.Execute(message);

                succeeded = true;
            }
            finally
            {
                await ProcessDataAsync(message);
                PublishDeprovisionResources(message);
            }

            return succeeded;
        }

        //private async Task SetupDeepScanProspectsForRepliesAsync(AllInOneVirtualAssistantMessageBody message)
        //{
        //    if (message.DeepScanProspectsForReplies != null)
        //    {
        //        _orchestrator.ProspectsThatRepliedDetected += OnProspectsThatRepliesDetected;

        //        NetworkProspectsResponse networkProspects = await _service.GetAllProspectsFromActiveCampaignsAsync(message);
        //        if (networkProspects == null || networkProspects.Items.Count == 0)
        //        {
        //            _logger.LogDebug("No network prospects were retrieved. {0} will not be triggered", nameof(DeepScanProspectsForRepliesBody));
        //            message.DeepScanProspectsForReplies = null;
        //        }
        //        else
        //        {
        //            message.DeepScanProspectsForReplies.NetworkProspects = networkProspects.Items;
        //        }
        //    }
        //}

        private void SetupCheckOffHoursConnections(AllInOneVirtualAssistantMessageBody message)
        {
            if (message.CheckOffHoursNewConnections != null)
            {
                _orchestrator.OffHoursNewConnectionsDetected += OnOffHoursNewConnectionsDetected;
            }
        }

        //private async Task GetFollowUpMessagesAsync(AllInOneVirtualAssistantMessageBody message)
        //{
        //    FollowUpMessagesResponse followUpMessages = await _service.GetFollowUpMessagesAsync(message);
        //    if (followUpMessages != null && followUpMessages.Items != null)
        //    {
        //        message.FollowUpMessages = new Queue<FollowUpMessageBody>(followUpMessages.Items);
        //    }
        //}

        //private async Task GetNetworkingMessagesAsync(AllInOneVirtualAssistantMessageBody message)
        //{
        //    // see if there are any networking messages that need to go out
        //    NetworkingMessagesResponse networkingMessages = await _service.GetNetworkingMessagesAsync(message);

        //    if (networkingMessages != null && networkingMessages.Items != null)
        //    {
        //        // fetch search urls
        //        message.NetworkingMessages = new Queue<NetworkingMessageBody>(networkingMessages.Items);
        //        await GetNetworkingSearchUrlsAsync(message);
        //    }
        //}

        public async Task GetNetworkingSearchUrlsAsync(AllInOneVirtualAssistantMessageBody message)
        {
            ParallelOptions parallelOptions = new()
            {
                MaxDegreeOfParallelism = 3
            };
            if (message.NetworkingMessages?.Count > 0)
            {
                await Parallel.ForEachAsync(message.NetworkingMessages, parallelOptions, async (networkingMessage, ct) =>
                {
                    GetSearchUrlProgressResponse response = await _service.GetSearchUrlProgressAsync(message);
                    if (response != null && response.SearchUrls != null && response.SearchUrls.Count > 0)
                    {
                        networkingMessage.SearchUrlsProgress = response.SearchUrls;
                    }
                });
            }
        }

        private async Task ProcessDataAsync(AllInOneVirtualAssistantMessageBody message)
        {
            // perform all async calls now.
            IList<ConnectionSentModel> connectionSents = _orchestrator.ConnectionsSent;
            if (connectionSents?.Count > 0)
            {
                await _service.ProcessSentConnectionsAsync(connectionSents, message);
            }

            IList<SearchUrlProgressModel> searchUrlsProgress = _orchestrator.UpdatedSearchUrlsProgress;
            if (searchUrlsProgress?.Count > 0)
            {
                await _service.UpdateSearchUrlsAsync(searchUrlsProgress, message);
            }

            List<PersistPrimaryProspectModel> persistPrimaryProspects = _orchestrator.PersistPrimaryProspects;
            if (persistPrimaryProspects?.Count > 0)
            {
                await _service.ProcessProspectListAsync(persistPrimaryProspects, message);
            }

            bool monthlyLimitReached = _orchestrator.MonthlySearchLimitReached;
            await _service.UpdateMonthlySearchLimitAsync(monthlyLimitReached, message);
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

        private async Task OnProspectsThatRepliesDetected(object sender, ProspectsThatRepliedEventArgs e)
        {
            _logger.LogInformation("Executing {0}. Preparing request to update all of the prospects who replied. HalId {1}. This is executed from {2} ", nameof(AllInOneVirtualAssistantMessageBody), e.Message.HalId, nameof(DeepScanProspectsForRepliesBody));
            await _service.ProcessCampaignProspectsThatRepliedAsync(e.Prospects, e.Message);
        }

        private async Task OnOffHoursNewConnectionsDetected(object sender, OffHoursNewConnectionsEventArgs e)
        {
            _logger.LogDebug("Executing {0}. New prospects have been detected from {1}. Sending them to the server for processing. HalId {2}", nameof(AllInOneVirtualAssistantMessageBody), nameof(CheckOffHoursNewConnectionsBody), e.Message.HalId);
            await _service.ProcessRecentlyAddedProspectsAsync(e.RecentlyAddedProspects, e.Message);
        }

        private async Task OnFollowUpMessagesSent(object sender, FollowUpMessagesSentEventArgs e)
        {
            _logger.LogDebug("Executing {0}. New prospects have been detected from {1}. Sending them to the server for processing. HalId {2}", nameof(AllInOneVirtualAssistantMessageBody), nameof(FollowUpMessageBody), e.Message.HalId);
            IList<SentFollowUpMessageModel> sentFollowUpMessages = _orchestrator.SentFollowUpMessages;
            ParallelOptions parallelOptions = new()
            {
                MaxDegreeOfParallelism = 3
            };
            if (sentFollowUpMessages?.Count > 0)
            {
                await Parallel.ForEachAsync(sentFollowUpMessages, parallelOptions, async (sentFollowUpMessage, ct) =>
                {
                    await _service.ProcessSentFollowUpMessageAsync(sentFollowUpMessage, e.Message);
                });
            }
        }
    }
}
