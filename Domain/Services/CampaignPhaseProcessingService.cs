using Domain.Services.Interfaces;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Requests.FromHal;
using Leadsly.Application.Model.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Services
{
    public class CampaignPhaseProcessingService : ICampaignPhaseProcessingService
    {
        public CampaignPhaseProcessingService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        private readonly HttpClient _httpClient;
        public Task<HalOperationResult<T>> ProcessNewConnectionsAsync<T>(NewConnectionRequest request, CancellationToken ct = default) where T : IOperationResponse
        {
            throw new NotImplementedException();
        }
    }
}
