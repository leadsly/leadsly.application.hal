﻿using Domain.Models.DeepScanProspectsForReplies;
using Domain.Models.FollowUpMessage;
using Domain.Models.MonitorForNewProspects;
using Domain.Models.Networking;
using Domain.Models.ProspectList;
using Domain.Models.Requests.MonitorForNewConnections;
using Domain.Models.Responses;
using Domain.Models.ScanProspectsForReplies;
using Domain.Models.SendConnections;
using Domain.MQ.Messages;
using Domain.Services.Interfaces;
using Domain.Services.Interfaces.Api;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Services
{
    public class AllInOneVirtualAssistantService : IAllInOneVirtualAssistantService
    {
        public AllInOneVirtualAssistantService(
            INetworkingService networkingService,
            IScanProspectsService scanProspectsService,
            IMonitorForNewConnectionsService monitorForNewConnectionsService,
            IFollowUpMessageService followUpMessageService,
            IDeepScanProspectsService deepScanProspectsService,
            ILogger<AllInOneVirtualAssistantService> logger,
            IAllInOneVirtualAssistantServiceApi api)
        {
            _deepScanProspectsService = deepScanProspectsService;
            _networkingService = networkingService;
            _scanProspectsService = scanProspectsService;
            _monitorForNewConnectionsService = monitorForNewConnectionsService;
            _followUpMessageService = followUpMessageService;
            _logger = logger;
            _api = api;
        }

        private readonly IDeepScanProspectsService _deepScanProspectsService;
        private readonly INetworkingService _networkingService;
        private readonly IScanProspectsService _scanProspectsService;
        private readonly IMonitorForNewConnectionsService _monitorForNewConnectionsService;
        private readonly IFollowUpMessageService _followUpMessageService;
        private readonly ILogger<AllInOneVirtualAssistantService> _logger;
        private readonly IAllInOneVirtualAssistantServiceApi _api;

        #region AllInOneVirtualAssistant

        public async Task<ConnectedNetworkProspectsResponse> GetAllPreviouslyConnectedNetworkProspectsAsync(PublishMessageBody message, CancellationToken ct = default)
        {
            PreviouslyConnectedNetworkProspectsRequest request = new()
            {
                NamespaceName = message.NamespaceName,
                ServiceDiscoveryName = message.ServiceDiscoveryName,
                RequestUrl = $"MonitorForNewProspects/{message.HalId}/previous-prospects"
            };

            HttpResponseMessage rawMessage = await _api.GetAllPreviouslyConnectedNetworkProspectsAsync(request, ct);

            if (rawMessage == null)
            {
                _logger.LogError("Response from application server was null. The request was responsible for getting previously connected prospects and previous number of total connections");
            }

            string content = await rawMessage.Content.ReadAsStringAsync();
            if (rawMessage.IsSuccessStatusCode == false)
            {
                _logger.LogError("Response from application server was null. The request was responsible for getting previously connected prospects and previous number of total connections. Response content {content}", content);
                return null;
            }

            ConnectedNetworkProspectsResponse response = default;
            try
            {
                response = JsonConvert.DeserializeObject<ConnectedNetworkProspectsResponse>(content);
                _logger.LogDebug("Successfully deserialized {0}", typeof(ConnectedNetworkProspectsResponse).Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deserialize {0}. Returning an explicit null", typeof(ConnectedNetworkProspectsResponse).Name);
            }

            return response;
        }

        public async Task UpdatePreviouslyConnectedNetworkProspectsAsync(PublishMessageBody message, IList<RecentlyAddedProspectModel> items, int previousTotalConnectionsCount, CancellationToken ct)
        {
            UpdateConnectedNetworkProspectsRequest request = new()
            {
                NamespaceName = message.NamespaceName,
                ServiceDiscoveryName = message.ServiceDiscoveryName,
                RequestUrl = $"MonitorForNewProspects/{message.HalId}/previous-prospects",
                Items = items,
                TotalConnectionsCount = previousTotalConnectionsCount
            };

            HttpResponseMessage rawMessage = await _api.UpdatePreviouslyConnectedNetworkProspectsAsync(request, ct);

            if (rawMessage == null)
            {
                _logger.LogError("Response from application server was null. The request was responsible for updating previously connected prospects and previous number of total connections");
            }

            if (rawMessage.IsSuccessStatusCode == false)
            {
                string content = await rawMessage.Content.ReadAsStringAsync();
                _logger.LogError("Response from application server was null. The request was responsible for updating previously connected prospects and previous number of total connections. Response content {content}", content);
            }
        }

        #endregion

        #region Networking

        public async Task<GetSearchUrlProgressResponse> GetSearchUrlProgressAsync(PublishMessageBody message, CancellationToken ct = default)
        {
            _logger.LogTrace("Executing {0}. This is for {1}", nameof(GetSearchUrlProgressAsync), nameof(NetworkingMessageBody));
            NetworkingMessageBody networkingMessage = message as NetworkingMessageBody;
            return await _networkingService.GetSearchUrlProgressAsync(networkingMessage, ct);
        }

        public async Task ProcessSentConnectionsAsync(IList<ConnectionSentModel> items, PublishMessageBody message, CancellationToken ct = default)
        {
            _logger.LogTrace("Executing {0}. This is for {1}", nameof(ProcessSentConnectionsAsync), nameof(NetworkingMessageBody));
            NetworkingMessageBody networkingMessage = message as NetworkingMessageBody;
            await _networkingService.ProcessSentConnectionsAsync(items, networkingMessage, ct);
        }

        public async Task UpdateSearchUrlsAsync(IList<SearchUrlProgressModel> items, PublishMessageBody message, CancellationToken ct = default)
        {
            _logger.LogTrace("Executing {0}. This is for {1}", nameof(UpdateSearchUrlsAsync), nameof(NetworkingMessageBody));
            NetworkingMessageBody networkingMessage = message as NetworkingMessageBody;
            await _networkingService.UpdateSearchUrlsAsync(items, networkingMessage, ct);
        }

        public async Task ProcessProspectListAsync(IList<PersistPrimaryProspectModel> items, PublishMessageBody message, CancellationToken ct = default)
        {
            _logger.LogTrace("Executing {0}. This is for {1}", nameof(ProcessProspectListAsync), nameof(NetworkingMessageBody));
            NetworkingMessageBody networkingMessage = message as NetworkingMessageBody;
            await _networkingService.ProcessProspectListAsync(items, networkingMessage, ct);
        }

        public async Task UpdateMonthlySearchLimitAsync(bool limitReached, PublishMessageBody message, CancellationToken ct = default)
        {
            _logger.LogTrace("Executing {0}. This is for {1}", nameof(UpdateMonthlySearchLimitAsync), nameof(NetworkingMessageBody));
            NetworkingMessageBody networkingMessage = message as NetworkingMessageBody;
            await _networkingService.UpdateMonthlySearchLimitAsync(limitReached, networkingMessage, ct);
        }

        public async Task<NetworkingMessagesResponse> GetNetworkingMessagesAsync(PublishMessageBody message, CancellationToken ct = default)
        {
            _logger.LogTrace("Executing {0}. This is for {1}", nameof(GetNetworkingMessagesAsync), nameof(NetworkingMessageBody));
            return await _networkingService.GetNetworkingMessagesAsync(message, ct);
        }

        #endregion

        #region ScanProspectsForReplies

        public async Task ProcessNewMessagesAsync(IList<NewMessageModel> items, PublishMessageBody message, CancellationToken ct = default)
        {
            _logger.LogTrace("Executing {0}. This is for {1}", nameof(ProcessNewMessagesAsync), nameof(ScanProspectsForRepliesBody));
            await _scanProspectsService.ProcessNewMessagesAsync(items, message, ct);
        }

        #endregion        

        #region MonitorForNewConnections

        public async Task ProcessRecentlyAddedProspectsAsync(IList<RecentlyAddedProspectModel> requests, PublishMessageBody message, CancellationToken ct = default)
        {
            _logger.LogTrace("Executing {0}. This is for {1}", nameof(ProcessRecentlyAddedProspectsAsync), nameof(MonitorForNewAcceptedConnectionsBody));
            await _monitorForNewConnectionsService.ProcessRecentlyAddedProspectsAsync(requests, message, ct);
        }

        #endregion

        #region FollowUpMessages

        public async Task ProcessSentFollowUpMessageAsync(SentFollowUpMessageModel item, PublishMessageBody message, CancellationToken ct = default)
        {
            _logger.LogTrace("Executing {0}. This is for {1}", nameof(ProcessSentFollowUpMessageAsync), nameof(FollowUpMessageBody));
            FollowUpMessageBody followUpMessageBody = message as FollowUpMessageBody;
            await _followUpMessageService.ProcessSentFollowUpMessageAsync(item, followUpMessageBody, ct);
        }

        public async Task<FollowUpMessagesResponse> GetFollowUpMessagesAsync(PublishMessageBody message, CancellationToken ct = default)
        {
            _logger.LogTrace("Executing {0}. This is for {1}", nameof(GetFollowUpMessagesAsync), nameof(FollowUpMessageBody));
            return await _followUpMessageService.GetFollowUpMessagesAsync(message, ct);
        }

        #endregion

        #region DeepScanProspectsForReplies

        public async Task<NetworkProspectsResponse> GetAllProspectsFromActiveCampaignsAsync(PublishMessageBody message, CancellationToken ct = default)
        {
            _logger.LogTrace("Executing {0}. This is for {1}", nameof(ProcessSentFollowUpMessageAsync), nameof(DeepScanProspectsForRepliesBody));
            DeepScanProspectsForRepliesBody deepScanProspectsMessage = message as DeepScanProspectsForRepliesBody;
            return await _deepScanProspectsService.GetAllProspectsFromActiveCampaignsAsync(deepScanProspectsMessage, ct);
        }

        public async Task ProcessCampaignProspectsThatRepliedAsync(IList<ProspectRepliedModel> prospects, PublishMessageBody message, CancellationToken ct = default)
        {
            _logger.LogTrace("Executing {0}. This is for {1}", nameof(ProcessSentFollowUpMessageAsync), nameof(AllInOneVirtualAssistantMessageBody));
            await _deepScanProspectsService.ProcessCampaignProspectsThatRepliedAsync(prospects, message, ct);
        }

        #endregion
    }
}
