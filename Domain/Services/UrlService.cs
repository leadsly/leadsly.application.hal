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
        public UrlService(ILogger<UrlService> logger, IWebHostEnvironment env, IOptions<AppServerConfigOptions> appServerConfigOptions)
        {
            _logger = logger;
            _env = env;
            _appServerConfigOptions = appServerConfigOptions.Value;
        }

        private readonly AppServerConfigOptions _appServerConfigOptions;
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
                url = $"http://{hostName}:{port}";
            }
            else if (_env.IsStaging())
            {
                string hostName = _appServerConfigOptions.HostName;
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
