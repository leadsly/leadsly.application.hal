using Domain.Facades.Interfaces;
using Domain.Providers.Campaigns.Interfaces;
using Domain.Services.Interfaces;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.Campaigns.Interfaces;
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
            ICampaignProvider campaignProvider,            
            IFollowUpMessagesProvider followUpMessagesProvider, 
            IProspectListProvider prospectListProvider,
            ISendConnectionsProvider sendConnectionsProvider,
            IMonitorForNewProspectsProvider monitorForNewProspectsProvider)
        {
            _campaignProvider = campaignProvider;
            _followUpMessagesProvider = followUpMessagesProvider;            
            _monitorForNewProspectsProvider = monitorForNewProspectsProvider;
            _prospectListProvider = prospectListProvider;
            _sendConnectionsProvider = sendConnectionsProvider;
            _logger = logger;
        }

        private readonly IMonitorForNewProspectsProvider _monitorForNewProspectsProvider;
        private readonly ICampaignProvider _campaignProvider;
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

            result = await _campaignProvider.PersistProspectListAsync<T>(result.Value, message);
            if(result.Succeeded == false)
            {
                return result;
            }

            return await _campaignProvider.TriggerSendConnectionsPhaseAsync<T>(message);
        }

        public async Task<HalOperationResult<T>> ExecutePhaseAsync<T>(SendConnectionsBody message) where T : IOperationResponse
        {
            HalOperationResult<T> result = new();
            // grab latest status of the connection request urls
            result = await _campaignProvider.GetLatestSendConnectionsUrlStatusesAsync<T>(message);
            if(result.Succeeded == false)
            {
                return result;
            }

            IGetSentConnectionsUrlStatusPayload getSentConnectionsUrlStatusPayload = ((IGetSentConnectionsUrlStatusPayload)result.Value);
            if(getSentConnectionsUrlStatusPayload == null)
            {
                return result;
            }

            if(getSentConnectionsUrlStatusPayload.SentConnectionsUrlStatuses.Count == 0)
            {
                // there are no more urls left to crawl
                _logger.LogInformation("There aren't any search urls left to crawl");
                // mark campaign as expired and inactive, if this request here fails for whatever reason
                // don't handle it just fire and forget
                await _campaignProvider.MarkCampaignExhaustedAsync<T>(message);

                // just return a success
                result.Succeeded = true;
                return result;
            }

            result = _sendConnectionsProvider.ExecutePhase<T>(message, getSentConnectionsUrlStatusPayload.SentConnectionsUrlStatuses);
            if(result.Succeeded == false)
            {
                return result;
            }

            // update latest status of the connection request urls
            ISendConnectionsPayload sendConnectionspayload = ((ISendConnectionsPayload)result.Value);
            if(sendConnectionspayload == null)
            {
                return null;
            }

            result = await _campaignProvider.UpdateSendConnectionsUrlStatusesAsync<T>(sendConnectionspayload.SentConnectionsUrlStatuses, message);
            if(result.Succeeded == false)
            {
                return result;
            }

            return await _campaignProvider.ProcessConnectionRequestSentForCampaignProspectsAsync<T>(sendConnectionspayload.CampaignProspects, message);                        
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
