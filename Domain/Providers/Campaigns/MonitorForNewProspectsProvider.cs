using Domain.POMs;
using Domain.POMs.Pages;
using Domain.Providers.Campaigns.Interfaces;
using Domain.Providers.Interfaces;
using Domain.Serializers.Interfaces;
using Domain.Services.Interfaces;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.Campaigns.Interfaces;
using Leadsly.Application.Model.Entities;
using Leadsly.Application.Model.Requests.FromHal;
using Leadsly.Application.Model.Responses;
using Leadsly.Application.Model.Responses.Hal.Interfaces;
using Leadsly.Application.Model.WebDriver;
using Leadsly.Application.Model.WebDriver.Interfaces;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
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
            ICampaignSerializer serializer,
            ITimestampService timestampService,
            ILinkedInNavBar linkedInNavBar,
            ILinkedInNotificationsPage linkedInNotificationsPage,
            ICampaignPhaseProcessingService campaignProcessingPhase,
            ILinkedInHtmlParser linkedInHtmlParser,
            IHalOperationConfigurationProvider halConfigurationProvider)
        {
            _logger = logger;
            _timestampService = timestampService;
            _linkedInMyNetworkPage = linkedInMyNetworkPage;
            _campaignProcessingPhase = campaignProcessingPhase;
            _webDriverProvider = webDriverProvider;
            _linkedInNavBar = linkedInNavBar;
            _serializer = serializer;
            _linkedInHomePage = linkedInHomePage;
            _linkedInHtmlParser = linkedInHtmlParser;
            _halConfigurationProvider = halConfigurationProvider;
            _linkedInNotificationsPage = linkedInNotificationsPage;
            _halIdentity = halIdentity;
        }

        private readonly ITimestampService _timestampService;
        private readonly IWebDriverProvider _webDriverProvider;
        private readonly ILinkedInNavBar _linkedInNavBar;
        private readonly ILogger<MonitorForNewProspectsProvider> _logger;
        private readonly ILinkedInHomePage _linkedInHomePage;
        private readonly ILinkedInHtmlParser _linkedInHtmlParser;
        private readonly ILinkedInNotificationsPage _linkedInNotificationsPage;
        private readonly ILinkedInMyNetworkPage _linkedInMyNetworkPage;
        private readonly ICampaignSerializer _serializer;
        private readonly ICampaignPhaseProcessingService _campaignProcessingPhase;
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

            // first navigate to messages
            result = GoToPage<T>(webDriver, message.PageUrl);
            if (result.Succeeded == false)
            {
                string pageUrl = message.PageUrl;
                _logger.LogError("Failed to navigate to {pageUrl}", pageUrl);
                return result;
            }

            try
            {
                // grab the new notifications on initial page load
                await CheckForNewConnectionsOnPageLoad(webDriver, message);

                await MonitorForNewConnections(webDriver, message);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error occured while monitoring for new connections");
                throw;
            }
            finally
            {
                // whenever any exception occurs try to close out the web driver if not possible just remove it from the list
                _logger.LogInformation("Attempting to close down the web driver and remove it from the list of web drivers");
                _webDriverProvider.CloseBrowser<T>(BrowserPurpose.MonitorForNewAcceptedConnections);
            }

            result.Succeeded = true;
            return result;
        }

        private async Task CheckForNewConnectionsOnPageLoad(IWebDriver webDriver, MonitorForNewAcceptedConnectionsBody message)
        {
            IList<NewProspectConnectionRequest> newProspects = _linkedInNotificationsPage.GatherAllNewProspectInfo(webDriver, message.TimeZoneId);
            if(newProspects.Count > 0)
            {
                // fire off request to initiate follow up message phase
                await ProcessNewlyAcceptedProspects(newProspects, message);
            }
        }

        private async Task MonitorForNewConnections(IWebDriver webDriver, MonitorForNewAcceptedConnectionsBody message)
        {
            DateTimeOffset endOfWorkDayInZone = DateTimeOffset.FromUnixTimeSeconds(message.EndWorkTime);
            while (_timestampService.GetDateTimeNowWithZone(message.TimeZoneId) < endOfWorkDayInZone)
            {
                await LookForNewConnections(webDriver, message);
            }
        }

        private async Task LookForNewConnections(IWebDriver webDriver, MonitorForNewAcceptedConnectionsBody message)
        {
            // first grab any new notifications displayed in the view 

            bool areNewNotification = _linkedInNavBar.AreNewNotifications(webDriver);            

            if (areNewNotification == true)
            {
                IList<NewProspectConnectionRequest> newProspects = GrabNewlyConnectedProspectsInfo(webDriver, message);

                if (newProspects.Count > 0)
                {
                    await ProcessNewlyAcceptedProspects(newProspects, message);
                }
            }
        }

        private async Task ProcessNewlyAcceptedProspects(IList<NewProspectConnectionRequest> newProspects, MonitorForNewAcceptedConnectionsBody message)
        {
            // fire off request to initiate follow up message phase
            NewProspectsConnectionsAcceptedRequest request = new()
            {
                HalId = message.HalId,
                NamespaceName = message.NamespaceName,
                ServiceDiscoveryName = message.ServiceDiscoveryName,
                RequestUrl = "api/campaignphases/process-newly-accepted-prospects",
                NewAcceptedProspectsConnections = newProspects,
                ApplicationUserId = message.UserId
            };

            await _campaignProcessingPhase.ProcessNewlyAcceptedProspectsAsync(request);
        }


        private IList<NewProspectConnectionRequest> GrabNewlyConnectedProspectsInfo(IWebDriver webDriver, MonitorForNewAcceptedConnectionsBody message)            
        {
            IList<NewProspectConnectionRequest> newProspectInfoRequests = new List<NewProspectConnectionRequest>();

            HalOperationResult<IOperationResponse> result = _linkedInNotificationsPage.ClickNewNotificationsButton<IOperationResponse>(webDriver);
            // if the button was not clicked successfully, or it wasn't found
            if(result.Succeeded == false)
            {
                // if web driver failed to locate or click new notifications button, click Notifications tab directly
                result = _linkedInNavBar.ClickNotificationsTab<IOperationResponse>(webDriver);
                if (result.Succeeded == false)
                {
                    return newProspectInfoRequests;
                }
            }

            newProspectInfoRequests = _linkedInNotificationsPage.GatherAllNewProspectInfo(webDriver, message.TimeZoneId);
            return newProspectInfoRequests;
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
