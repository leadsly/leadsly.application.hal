using Domain.Facades.Interfaces;
using Domain.Providers.Campaigns.Interfaces;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.RabbitMQ;
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
            IFollowUpMessagesProvider followUpMessagesProvider, 
            IMonitorForNewProspectsProvider monitorForNewProspectsProvider)
        {
            _followUpMessagesProvider = followUpMessagesProvider;            
            _monitorForNewProspectsProvider = monitorForNewProspectsProvider;
            _logger = logger;
        }

        private readonly IMonitorForNewProspectsProvider _monitorForNewProspectsProvider;
        private readonly IFollowUpMessagesProvider _followUpMessagesProvider;
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

        public HalOperationResult<T> ExecutePhase<T>(ProspectListBody message) where T : IOperationResponse
        {
            throw new NotImplementedException();
        }

        public HalOperationResult<T> ExecutePhase<T>(SendConnectionRequestsBody message) where T : IOperationResponse
        {
            throw new NotImplementedException();
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
