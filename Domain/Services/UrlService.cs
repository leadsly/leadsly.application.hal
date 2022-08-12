using Domain.Services.Interfaces;
using Leadsly.Application.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Domain.Services
{
    public class UrlService : IUrlService
    {
        public UrlService(ILogger<UrlService> logger, IWebHostEnvironment env, IOptions<AppServerConfigOptions> appServerConfigOptions, IOptions<SidecartServerConfigOptions> sidecartConfigOptions)
        {
            _logger = logger;
            _env = env;
            _appServerConfigOptions = appServerConfigOptions.Value;
            _sidecartConfigOptions = sidecartConfigOptions.Value;
        }

        private readonly AppServerConfigOptions _appServerConfigOptions;
        private readonly SidecartServerConfigOptions _sidecartConfigOptions;
        private readonly ILogger<UrlService> _logger;
        private readonly IWebHostEnvironment _env;

        public string GetBaseServerUrl(string serviceDiscoveryName, string namespaceName)
        {
            _logger.LogInformation("Getting server's url");
            string url = string.Empty;
            if (_env.IsDevelopment())
            {
                string hostName = _appServerConfigOptions.HostName;
                long port = _appServerConfigOptions.Port;
                url = $"https://{hostName}:{port}/api";
            }
            else if (_env.IsStaging())
            {
                string hostName = _appServerConfigOptions.HostName;
                url = $"http://{hostName}/api";
            }
            else
            {
                url = $"http://{serviceDiscoveryName}.{namespaceName}/api";
            }

            _logger.LogDebug("Final app server url is {url}", url);
            return url;
        }

        public string GetBaseGridUrl(string serviceDiscoveryName, string namespaceName)
        {
            _logger.LogInformation("Getting server's url");
            string url = string.Empty;
            if (_env.IsDevelopment())
            {
                string hostName = _sidecartConfigOptions.HostName;
                long port = _sidecartConfigOptions.Port;
                url = $"https://{hostName}:{port}/api";
            }
            else if (_env.IsStaging())
            {
                string hostName = _sidecartConfigOptions.HostName;
                url = $"http://{hostName}/api";
            }
            else
            {
                url = $"http://{serviceDiscoveryName}.{namespaceName}/api";
            }

            _logger.LogDebug("Final grid and sidecart url is {url}", url);
            return url;
        }
    }
}
