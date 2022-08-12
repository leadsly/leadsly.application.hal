using Domain.Models.Requests;
using Domain.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Services
{
    public class LeadslyGridSidecartService : ILeadslyGridSidecartService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<LeadslyGridSidecartService> _logger;
        private readonly IUrlService _urlService;

        public LeadslyGridSidecartService(HttpClient httpClient, ILogger<LeadslyGridSidecartService> logger, IUrlService urlService)
        {
            _httpClient = httpClient;
            _urlService = urlService;
            _logger = logger;
        }

        public async Task<HttpResponseMessage> CloneChromeProfileAsync(CloneChromeProfileRequest request, CancellationToken ct = default)
        {
            string url = _urlService.GetBaseGridUrl(request.GridNamespaceName, request.GridServiceDiscoveryName);

            HttpRequestMessage req = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{url}/{request.RequestUrl}", UriKind.Absolute),
                Content = JsonContent.Create(new
                {
                    NewChromeProfile = request.NewChromeProfile,
                    DefaultChromeUserProfilesDir = request.DefaultChromeUserProfilesDir,
                    DefaultChromeProfileName = request.DefaultChromeProfileName,
                    ProfilesVolume = request.ProfilesVolume,
                    UseGrid = request.UseGrid
                })
            };

            HttpResponseMessage response = default;
            try
            {
                _logger.LogDebug("Sending request to clone chrome profile");
                response = await _httpClient.SendAsync(req, ct);
                _logger.LogDebug("Finished sending request to clone chrome profile");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send request to clone chrome profile");
            }

            return response;
        }
    }
}
