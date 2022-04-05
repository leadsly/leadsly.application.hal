﻿using Domain.Facades.Interfaces;
using Domain.Providers.Campaigns.Interfaces;
using Domain.Services.Interfaces;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.RabbitMQ;
using Leadsly.Application.Model.Requests.FromHal;
using Leadsly.Application.Model.Responses;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Facades
{
    public class CampaignPhaseFacade : ICampaignPhaseFacade
    {
        public CampaignPhaseFacade(ILogger<CampaignPhaseFacade> logger,
            ICampaignProcessingProvider campaignProcessingProvider,            
            IFollowUpMessagesProvider followUpMessagesProvider, 
            IProspectListProvider prospectListProvider,
            ISendConnectionsProvider sendConnectionsProvider,
            IMonitorForNewProspectsProvider monitorForNewProspectsProvider)
        {
            _campaignProcessingProvider = campaignProcessingProvider;
            _followUpMessagesProvider = followUpMessagesProvider;            
            _monitorForNewProspectsProvider = monitorForNewProspectsProvider;
            _prospectListProvider = prospectListProvider;
            _sendConnectionsProvider = sendConnectionsProvider;
            _logger = logger;
        }

        private readonly IMonitorForNewProspectsProvider _monitorForNewProspectsProvider;
        private readonly ICampaignProcessingProvider _campaignProcessingProvider;
        private readonly IFollowUpMessagesProvider _followUpMessagesProvider;
        private readonly IProspectListProvider _prospectListProvider;
        private readonly ISendConnectionsProvider _sendConnectionsProvider;
        private readonly ILogger<CampaignPhaseFacade> _logger;        

        public HalOperationResult<T> ExecuteFollowUpMessagesPhase<T>(FollowUpMessagesBody message)
            where T : IOperationResponse
        {
            return _followUpMessagesProvider.ExecutePhase<T>(message);
        }

        public HalOperationResult<T> ExecutePhase<T>(ScanProspectsForRepliesBody message) where T : IOperationResponse
        {
            throw new NotImplementedException();
        }

        public async Task<HalOperationResult<T>> ExecutePhase<T>(MonitorForNewAcceptedConnectionsBody message) where T : IOperationResponse
        {
            return await _monitorForNewProspectsProvider.ExecutePhase<T>(message);
        }

        public async Task<HalOperationResult<T>> ExecutePhaseAsync<T>(ProspectListBody message) where T : IOperationResponse
        {
            HalOperationResult<T> result = _prospectListProvider.ExecutePhase<T>(message);
            if(result.Succeeded == false)
            {
                return result;                
            }

            result = await _campaignProcessingProvider.PersistProspectListAsync<T>(result.Value, message);
            if(result.Succeeded == false)
            {
                return result;
            }

            return await _campaignProcessingProvider.TriggerSendConnectionsPhaseAsync<T>(message);
        }

        public async Task<HalOperationResult<T>> ExecutePhaseAsync<T>(SendConnectionsBody message) where T : IOperationResponse
        {
            HalOperationResult<T> result = _sendConnectionsProvider.ExecutePhase<T>(message);
            if(result.Succeeded == false)
            {
                return result;
            }

            return await _campaignProcessingProvider.ProcessConnectionRequestSentForCampaignProspectsAsync<T>(result.Value, message);                        
        }

        public HalOperationResult<T> ExecutePhase<T>(ConnectionWithdrawBody message) where T : IOperationResponse
        {
            throw new NotImplementedException();
        }

        public HalOperationResult<T> ExecutePhase<T>(FollowUpMessagesBody message) where T : IOperationResponse
        {
            throw new NotImplementedException();
        }
    }
}
