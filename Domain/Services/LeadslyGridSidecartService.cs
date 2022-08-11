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

        public LeadslyGridSidecartService(HttpClient httpClient, ILogger<LeadslyGridSidecartService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<HttpResponseMessage> CloneChromeProfileAsync(CloneChromeProfileRequest request, CancellationToken ct = default)
        {
            HttpRequestMessage req = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{request.BaseUrl}/{request.Endpoint}"),
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
                response = await _httpClient.SendAsync(req, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send request to clone chrome profile");
            }

            return response;
        }
    }
}
