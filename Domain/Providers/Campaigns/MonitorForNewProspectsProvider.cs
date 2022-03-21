using Domain.POMs;
using Domain.POMs.Pages;
using Domain.Providers.Campaigns.Interfaces;
using Domain.Providers.Interfaces;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.Entities;
using Leadsly.Application.Model.Responses;
using Leadsly.Application.Model.WebDriver;
using Leadsly.Application.Model.WebDriver.Interfaces;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using System;
using System.Threading.Tasks;

namespace Domain.Providers.Campaigns
{
    public class MonitorForNewProspectsProvider : IMonitorForNewProspectsProvider
    {
        public MonitorForNewProspectsProvider(
            ILinkedInHomePage linkedInHomePage,
            ILinkedInMyNetworkPage linkedInMyNetworkPage,
            ILogger<MonitorForNewProspectsProvider> logger, 
            IWebDriverProvider webDriverProvider,
            IHalIdentity halIdentity,
            ILinkedInNavBar linkedInNavBar,
            IHalOperationConfigurationProvider halConfigurationProvider)
        {
            _logger = logger;
            _linkedInMyNetworkPage = linkedInMyNetworkPage;
            _webDriverProvider = webDriverProvider;
            _linkedInNavBar = linkedInNavBar;
            _linkedInHomePage = linkedInHomePage;
            _halConfigurationProvider = halConfigurationProvider;
            _halIdentity = halIdentity;
        }

        private readonly IWebDriverProvider _webDriverProvider;
        private readonly ILinkedInNavBar _linkedInNavBar;
        private readonly ILogger<MonitorForNewProspectsProvider> _logger;
        private readonly ILinkedInHomePage _linkedInHomePage;
        private readonly ILinkedInMyNetworkPage _linkedInMyNetworkPage;
        private readonly IHalOperationConfigurationProvider _halConfigurationProvider;
        private readonly IHalIdentity _halIdentity;

        public async Task<HalOperationResult<T>> ExecutePhase<T>(MonitorForNewAcceptedConnectionsBody message) where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            // assume user is authenticated
            WebDriverOperationData operationData = new()
            {
                BrowserPurpose = BrowserPurpose.MonitorForNewAcceptedConnections,
                ChromeProfileName = message.ChromeProfileName,
                PageUrl = message.PageUrl
            };

            HalOperationResult<T> driverOperationResult = _webDriverProvider.GetOrCreateWebDriver<T>(operationData);

            if (driverOperationResult.Succeeded == false)
            {
                _logger.LogWarning("There was an issue getting or creating webdriver instance");
                return result;
            }

            IWebDriver webDriver = ((IGetOrCreateWebDriverOperation)driverOperationResult.Value).WebDriver;

            return await MonitorForNewConnections<T>(webDriver, message);
        }

        private async Task<HalOperationResult<T>> MonitorForNewConnections<T>(IWebDriver webDriver, MonitorForNewAcceptedConnectionsBody message)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            // first navigate to messages
            result = GoToPage<T>(webDriver, message.PageUrl);
            if (result.Succeeded == false)
            {
                string pageUrl = message.PageUrl;
                _logger.LogError("Failed to navigate to {pageUrl}", pageUrl);
                return result;
            }

            HalOperationConfiguration operationConfiguration = await _halConfigurationProvider.GetOperationConfigurationByIdAsync(_halIdentity.Id);

            TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(message.TimeZoneId) ?? TimeZoneInfo.Utc;
            DateTime dateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.Now, timeZone);
            while (TimeZoneInfo.ConvertTimeFromUtc(DateTime.Now, timeZone) > message.EndWorkTiem)
            {
                try
                {
                    result = LookForNewConnections<T>(webDriver);
                    if(result.Succeeded == false)
                    {
                        return result;
                    }
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, "Error occured executing MonitorForNewProspects phase.");                    
                    return result;
                }
            }

            result.Succeeded = true;
            return result;
        }

        private HalOperationResult<T> LookForNewConnections<T>(IWebDriver webDriver)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            result = _linkedInNavBar.IsNewConnectionNotification<T>(webDriver);
            if(result.Succeeded == false)
            {
                return result;
            }

            result.Succeeded = true;
            return result;
        }

        private HalOperationResult<T> GoToPage<T>(IWebDriver webDriver, string pageUrl)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            if (webDriver.Url.Contains(pageUrl) == false)
            {
                // first navigate to messages
                result = _linkedInHomePage.GoToPage<T>(webDriver, pageUrl);
            }
            else
            {
                result.Succeeded = true;
            }

            return result;
        }
    }
}
