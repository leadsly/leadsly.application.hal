using Domain.POMs;
using Domain.POMs.Pages;
using Domain.Providers.Campaigns.Interfaces;
using Domain.Providers.Interfaces;
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
            ILinkedInNavBar linkedInNavBar,
            ILinkedInNotificationsPage linkedInNotificationsPage,
            ICampaignPhaseProcessingService campaignProcessingPhase,
            ILinkedInHtmlParser linkedInHtmlParser,
            IHalOperationConfigurationProvider halConfigurationProvider)
        {
            _logger = logger;
            _linkedInMyNetworkPage = linkedInMyNetworkPage;
            _campaignProcessingPhase = campaignProcessingPhase;
            _webDriverProvider = webDriverProvider;
            _linkedInNavBar = linkedInNavBar;
            _linkedInHomePage = linkedInHomePage;
            _linkedInHtmlParser = linkedInHtmlParser;
            _halConfigurationProvider = halConfigurationProvider;
            _linkedInNotificationsPage = linkedInNotificationsPage;
            _halIdentity = halIdentity;
        }

        private readonly IWebDriverProvider _webDriverProvider;
        private readonly ILinkedInNavBar _linkedInNavBar;
        private readonly ILogger<MonitorForNewProspectsProvider> _logger;
        private readonly ILinkedInHomePage _linkedInHomePage;
        private readonly ILinkedInHtmlParser _linkedInHtmlParser;
        private readonly ILinkedInNotificationsPage _linkedInNotificationsPage;
        private readonly ILinkedInMyNetworkPage _linkedInMyNetworkPage;
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

            try
            {
                await MonitorForNewConnections<T>(webDriver, message);
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

            // HalOperationConfiguration operationConfiguration = await _halConfigurationProvider.GetOperationConfigurationByIdAsync(_halIdentity.Id);

            TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(message.TimeZoneId) ?? TimeZoneInfo.Utc;
            DateTimeOffset endOfWorkDay = DateTimeOffset.FromUnixTimeSeconds(message.EndWorkTime);
            while (TimeZoneInfo.ConvertTimeFromUtc(DateTime.Now, timeZone) > endOfWorkDay)
            {
                try
                {
                    result = await LookForNewConnections<T>(webDriver, message);
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

        private async Task<HalOperationResult<T>> LookForNewConnections<T>(IWebDriver webDriver, MonitorForNewAcceptedConnectionsBody message)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            result = _linkedInNavBar.IsNewNotification<T>(webDriver);
            if(result.Succeeded == false)
            {
                return result;
            }

            bool? isNewNotification = ((INotificationNavBarControl)result.Value).NewNotification;

            // no new connections
            if(isNewNotification != null && isNewNotification == false)
            {
                result.Succeeded = true;
                return result;
            }

            // else we have a new notification lets try to click 'New notifications' button and grab all of the notifications
            return await GrabLatestNotifications<T>(webDriver, message);
        }

        private async Task<HalOperationResult<T>> GrabLatestNotifications<T>(IWebDriver webDriver, MonitorForNewAcceptedConnectionsBody message)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            result = _linkedInNotificationsPage.ClickNewNotificationsButton<T>(webDriver);

            if(result.Succeeded == false)
            {
                // if web driver failed to locate or click new notifications button, click Notifications tab directly
                result = _linkedInNavBar.ClickNotificationsTab<T>(webDriver);
                if (result.Succeeded == false)
                {
                    return result;
                }
            }

            result = _linkedInNotificationsPage.GatherAllNewProspectInfo<T>(webDriver);
            if(result.Succeeded == false)
            {
                return result;
            }

            INewProspectAcceptedPayload payload = ((INewProspectAcceptedPayload)result.Value);
            // here we need to send over the payload with new prospects and check if they come from any of our campaigns and then send them a message

            return await GatherNewlyConnectedProspects<T>(webDriver, newNotificationsCount, message);
        }

        private async Task<HalOperationResult<T>> GatherNewlyConnectedProspects<T>(IWebDriver webDriver, int newConnectionsCount, MonitorForNewAcceptedConnectionsBody message)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new();
            
            result = _linkedInMyNetworkPage.CollectAllNewConnections<T>(webDriver, newConnectionsCount);
            if(result.Succeeded == false)
            {
                return result;
            }

            IReadOnlyCollection<IWebElement> myNetworkNewConn = ((INewInvitationsMyNetwork)result.Value).NewConnections;
            // we need to send the new list of individuals to the server for processing and triggering send follow up message event
            result = _linkedInHtmlParser.ParseMyNetworkConnections<T>(myNetworkNewConn);
            if(result.Succeeded == false)
            {
                return result;
            }

            INewConnectionProspects newProspectsPayload = (INewConnectionProspects)result.Value;
            return await SendNewConnectionsPayloadAsync<T>(newProspectsPayload, message);
        }

        private async Task<HalOperationResult<T>> SendNewConnectionsPayloadAsync<T>(INewConnectionProspects payload, MonitorForNewAcceptedConnectionsBody message)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new();
            NewProspectConnectionRequest request = new()
            {
                ApiServerUrl = message.ApiServerUrl,
                NewConnectionProspects = payload.NewProspects,
                HalId = _halIdentity.Id
            };

            // fire and forget here
            HttpResponseMessage response = await _campaignProcessingPhase.ProcessNewConnectionsAsync(request);
            if(response == null)
            {
                result.Failures.Add(new()
                {
                    Code = Codes.HTTP_REQUEST_ERROR,
                    Reason = "Failed to send request to api server to process new connections",
                    Detail = "Connection error occured sending request"
                });
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
