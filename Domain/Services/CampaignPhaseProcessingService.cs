﻿using Domain.Services.Interfaces;
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
        private const string HttpPrefix = "http://";

        public async Task<HttpResponseMessage> ProcessNewConnectionsAsync(NewProspectConnectionRequest request, CancellationToken ct = default)
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

        public async Task<HttpResponseMessage> ProcessProspectListAsync(ProspectListPhaseCompleteRequest request, CancellationToken ct = default)
        {
            string apiServerUrl = "https://localhost:5001/api/prospect-list"; // $"{HttpPrefix}{request.ServiceDiscoveryName}.{request.NamespaceName}";

            HttpResponseMessage response = default;
            try
            {
                HttpRequestMessage req = new()
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(apiServerUrl, UriKind.Absolute),
                    Content = JsonContent.Create(new
                    {
                        PrimaryProspectListId = request.PrimaryProspectListId,
                        UserId = request.UserId,
                        HalId = request.HalId,
                        CampaignId = request.CampaignId,
                        Prospects = request.Prospects,

                    })
                };

                _logger.LogInformation("Sending request to process my new network connections.");
                response = await _httpClient.SendAsync(req, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send reequest to process my new network connections.");
            }

            return response;
        }

        public async Task<HttpResponseMessage> UpdateContactedCampaignProspectListAsync(CampaignProspectListRequest request, CancellationToken ct = default)
        {
            string apiServerUrl = $"https://localhost:5001/{request.RequestUrl}"; // $"{HttpPrefix}{request.ServiceDiscoveryName}.{request.NamespaceName}";

            HttpResponseMessage response = default;
            try
            {
                HttpRequestMessage req = new()
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(apiServerUrl, UriKind.Absolute),
                    Content = JsonContent.Create(new
                    {
                        UserId = request.UserId,
                        HalId = request.HalId,
                        CampaignId = request.CampaignId,
                        CampaignProspects = request.CampaignProspects
                    })
                };

                _logger.LogInformation("Sending request to process my new network connections.");
                response = await _httpClient.SendAsync(req, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send reequest to process my new network connections.");
            }

            return response;
        }

        public async Task<HttpResponseMessage> TriggerCampaignProspectListAsync(TriggerSendConnectionsRequest request, CancellationToken ct = default)
        {
            string apiServerUrl = $"https://localhost:5001/{request.RequestUrl}"; // $"{HttpPrefix}{request.ServiceDiscoveryName}.{request.NamespaceName}";

            HttpResponseMessage response = default;
            try
            {
                HttpRequestMessage req = new()
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(apiServerUrl, UriKind.Absolute),
                    Content = JsonContent.Create(new
                    {
                        CampaignId = request.CampaignId,
                        UserId = request.UserId,
                        HalId = request.HalId
                    })
                };

                _logger.LogInformation("Sending request to process my new network connections.");
                response = await _httpClient.SendAsync(req, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send reequest to process my new network connections.");
            }

            return response;
        }
    }
}
