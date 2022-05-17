using Domain.Services.Interfaces;
using Leadsly.Application.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Services
{
    public class UrlService : IUrlService
    {
        public UrlService(ILogger<UrlService> logger, IWebHostEnvironment env, IOptions<HalConfigOptions> halConfigOptions)
        {
            _logger = logger;
            _env = env;
            _halConfigOptions = halConfigOptions.Value;
        }

        private readonly HalConfigOptions _halConfigOptions;
        private readonly ILogger<UrlService> _logger;
        private readonly IWebHostEnvironment _env;

        public string GetBaseServerUrl(string serviceDiscoveryName, string namespaceName)
        {
            _logger.LogInformation("Getting server's url");
            string url = string.Empty;
            if (_env.IsDevelopment())
            {
                string hostName = _halConfigOptions.HostName;
                long port = _halConfigOptions.Port;
                url = $"http://{hostName}:{port}";
            }
            else if (_env.IsStaging())
            {
                string hostName = _halConfigOptions.HostName;
                url = $"http://{hostName}";
            }
            else
            {
                url = $"https://{serviceDiscoveryName}.{namespaceName}";
            }

            _logger.LogDebug("Final app server url is {url}", url);
            return url;
        }
    }
}
