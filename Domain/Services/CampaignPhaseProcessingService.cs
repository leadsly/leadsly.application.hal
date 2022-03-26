using Domain.Services.Interfaces;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Requests.FromHal;
using Leadsly.Application.Model.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Services
{
    public class CampaignPhaseProcessingService : ICampaignPhaseProcessingService
    {
        public CampaignPhaseProcessingService(HttpClient httpClient, ILogger<CampaignPhaseProcessingService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        private readonly HttpClient _httpClient;
        private readonly ILogger<CampaignPhaseProcessingService> _logger;

        public async Task<HttpResponseMessage> ProcessNewConnectionsAsync<T>(NewConnectionRequest request, CancellationToken ct = default) where T : IOperationResponse
        {
            string apiServerUrl = "http://localhost:5010"; //; request.ApiServerUrl;

            HttpRequestMessage req = new()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(apiServerUrl, UriKind.Relative),
                Content = JsonContent.Create(new
                {
                    NewConnectionProspects = request.NewConnectionProspects,
                    HalId = request.HalId
                })
            };

            HttpResponseMessage response = default;
            try
            {
                _logger.LogInformation("Sending request to process my new network connections.");
                response = await _httpClient.SendAsync(req, ct);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Failed to send reequest to process my new network connections.");
            }

            return response;
        }
    }
}
