using Domain.Providers.Campaigns.Interfaces;
using Domain.Services.Interfaces.Networking;
using Leadsly.Application.Model.Campaigns;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using System.Collections.Generic;

namespace Domain.Services.Networking
{
    public class SendConnectionsService : ISendConnectionsService
    {
        public SendConnectionsService(ILogger<SendConnectionsService> logger, IPhaseDataProcessingProvider phaseDataProcessingProvider)
        {
            _logger = logger;
            _phaseDataProcessingProvider = phaseDataProcessingProvider;
        }

        private readonly ILogger<SendConnectionsService> _logger;
        private readonly IPhaseDataProcessingProvider _phaseDataProcessingProvider;

        public bool SendConnections(IWebDriver webDriver, NetworkingMessageBody message, IList<IWebElement> connectableProspects)
        {
            //_logger.LogTrace("Executing SendConnections phase");
            //IList<CampaignProspectRequest> campaignProspectRequests = new List<CampaignProspectRequest>();
            //_logger.LogDebug("Number of connectable prospects: {0}", connectableProspects?.Count);
            //foreach (IWebElement connectableProspect in connectableProspects)
            //{
            //    if (numberOfConnectionsSent >= message.ProspectsToCrawl)
            //    {
            //        _logger.LogDebug("Number of connections sent has reached the limit. Number of connections sent is {0}. Number of connections to send out for this phase is {1}", numberOfConnectionsSent, message.ProspectsToCrawl);
            //        break;
            //    }

            //    CampaignProspectRequest request = ExecuteSendConnectionInternal(webDriver, connectableProspect, message.CampaignId);
            //    if (request != null)
            //    {
            //        campaignProspectRequests.Add(request);
            //        numberOfConnectionsSent += 1;
            //    }
            //    else
            //    {
            //        _logger.LogDebug("The CampaignProspectRequest is null. Skipping this prospect and moving onto the next one");
            //    }
            //}

            //if (campaignProspectRequests.Count > 0)
            //{
            //    result = await _phaseDataProcessingProvider.ProcessConnectionRequestSentForCampaignProspectsAsync<T>(campaignProspectRequests, message, message.CampaignId);
            //    if (result.Succeeded == false)
            //    {
            //        return result;
            //    }
            //}

            //result.Succeeded = true;
            //return result;
            return true;
        }
    }
}
