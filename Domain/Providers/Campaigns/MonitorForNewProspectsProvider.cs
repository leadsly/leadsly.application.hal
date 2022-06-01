using Domain.Facades.Interfaces;
using Domain.POMs;
using Domain.POMs.Pages;
using Domain.Providers.Campaigns.Interfaces;
using Domain.Providers.Interfaces;
using Domain.Serializers.Interfaces;
using Domain.Services.Interfaces;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.Campaigns.Interfaces;
using Leadsly.Application.Model.Campaigns.MonitorForNewProspects;
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
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Providers.Campaigns
{
    public class MonitorForNewProspectsProvider : IMonitorForNewProspectsProvider
    {
        public MonitorForNewProspectsProvider(                        
            ILogger<MonitorForNewProspectsProvider> logger, 
            IWebDriverProvider webDriverProvider,            
            ITimestampService timestampService,
            IScreenHouseKeeperService screenKeeperService,
            IHumanBehaviorService humanBehaviorService,            
            IPhaseDataProcessingService campaignProcessingPhase,            
            ILinkedInPageFacade linkedInPageFacade
            )
        {
            _screenHouseKeeperService = screenKeeperService;
            _logger = logger;
            _humanBehaviorService = humanBehaviorService;
            _linkedInPageFacade = linkedInPageFacade;
            _timestampService = timestampService;            
            _campaignProcessingPhase = campaignProcessingPhase;
            _webDriverProvider = webDriverProvider;            
        }

        private readonly IScreenHouseKeeperService _screenHouseKeeperService;
        private readonly IHumanBehaviorService _humanBehaviorService;
        private readonly ILinkedInPageFacade _linkedInPageFacade;
        private readonly ITimestampService _timestampService;
        private readonly IWebDriverProvider _webDriverProvider;        
        private readonly ILogger<MonitorForNewProspectsProvider> _logger;                
        private readonly IPhaseDataProcessingService _campaignProcessingPhase;
        private int PreviousConnectionsCount = 0;
        private IList<RecentlyAddedProspect> PreviousRecentlyAdded = new List<RecentlyAddedProspect>();
        public static bool IsRunning { get; private set; }        

        public async Task<HalOperationResult<T>> ExecutePhaseOffHoursScanPhaseAsync<T>(MonitorForNewAcceptedConnectionsBody message) where T : IOperationResponse
        {
            HalOperationResult<T> result = SetUpPhaseAsync<T>(message);
            if(result.Succeeded == false)
            {
                return result;
            }

            IWebDriver webDriver = ((IGetOrCreateWebDriverOperation)result.Value).WebDriver;

            try
            {
                result = await ScanForAnyNewOffHoursConnectionsAsync<T>(webDriver, message);
                if(result.Succeeded == false)
                {
                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occured while monitoring for new connections");
                throw;
            }
            finally
            {
                _logger.LogInformation("Finished running MonitorForNewAcceptedConnections phase from off hours");
            }
                        
            return result;
        }

        public async Task<HalOperationResult<T>> ExecutePhase<T>(MonitorForNewAcceptedConnectionsBody message) where T : IOperationResponse
        {
            HalOperationResult<T> result = SetUpPhaseAsync<T>(message);
            if (result.Succeeded == false)
            {
                return result;
            }

            IWebDriver webDriver = ((IGetOrCreateWebDriverOperation)result.Value).WebDriver;

            try
            {               
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

        private HalOperationResult<T> SetUpPhaseAsync<T>(MonitorForNewAcceptedConnectionsBody message)
            where T : IOperationResponse
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

            result.Value = driverOperationResult.Value;
            result.Succeeded = true;
            return result;
        }
        
        private async Task<HalOperationResult<T>> ScanForAnyNewOffHoursConnectionsAsync<T>(IWebDriver webDriver, MonitorForNewAcceptedConnectionsBody message)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new();
            // just grab all of the prospects from the given range of hours and send them to the server for processing
            IList<RecentlyAddedProspect> recentlyAddedProspects = _linkedInPageFacade.ConnectionsView.GetRecentlyAdded(webDriver, message.NumOfHoursAgo);

            await ProcessNewConnectionsAsync(recentlyAddedProspects, message);

            result.Succeeded = true;
            return result;
        }

        private async Task MonitorForNewConnections(IWebDriver webDriver, MonitorForNewAcceptedConnectionsBody message)
        {
            IsRunning = true;
            DateTimeOffset endOfWorkDayLocal = _timestampService.GetDateTimeOffsetLocal(message.TimeZoneId, message.EndWorkTime);            
            while (_timestampService.GetNowLocalized(message.TimeZoneId) < endOfWorkDayLocal)
            {
                PreviousConnectionsCount = _linkedInPageFacade.ConnectionsView.GetConnectionsCount(webDriver);
                PreviousRecentlyAdded = _linkedInPageFacade.ConnectionsView.GetAllRecentlyAdded(webDriver);

                _humanBehaviorService.RandomWaitMinutes(2, 5);

                CloseAllConversations(webDriver);

                HalOperationResult<IOperationResponse> result = _webDriverProvider.Refresh<IOperationResponse>(webDriver);
                if(result.Succeeded == false)
                {
                    break;
                }

                int connectionCount = _linkedInPageFacade.ConnectionsView.GetConnectionsCount(webDriver);
                if(connectionCount > PreviousConnectionsCount)
                {
                    IList<RecentlyAddedProspect> currentProspects = _linkedInPageFacade.ConnectionsView.GetAllRecentlyAdded(webDriver);
                    IList<RecentlyAddedProspect> newProspects = currentProspects.Where(p => PreviousRecentlyAdded.Any(prev => prev.Name == p.Name) == false).ToList();
                    await ProcessNewConnectionsAsync(newProspects, message);
                }
            }
            IsRunning = false;

            _logger.LogInformation("Stopping to look for new connections. MonitorForNewAcceptedConnections finished running because it is end of the work day");
        }

        private void CloseAllConversations(IWebDriver webDriver)
        {
            IReadOnlyCollection<IWebElement> closeButtons = _screenHouseKeeperService.GetAllConversationCardsCloseButtons(webDriver);
            foreach (IWebElement closeButton in closeButtons)
            {
                _humanBehaviorService.RandomWaitSeconds(1, 3);
                _screenHouseKeeperService.CloseConversation(closeButton);
            }
        }

        #region MonitorForNewConnections on MyNetwork Connections page

        private async Task ProcessNewConnectionsAsync(IList<RecentlyAddedProspect> prospects, MonitorForNewAcceptedConnectionsBody message)
        {
            IList<NewProspectConnectionRequest> newProspects = new List<NewProspectConnectionRequest>();
            foreach (RecentlyAddedProspect prospect in prospects)
            {
                // create timestamp with minues num of hours
                DateTimeOffset dateTimeOffsetLocalized = _timestampService.GetNowLocalized(message.TimeZoneId);
                DateTimeOffset connectionAcceptedTime = dateTimeOffsetLocalized.AddHours(-prospect.NumberOfHoursAgo);

                NewProspectConnectionRequest newProspect = new()
                {
                    ProspectName = prospect.Name,
                    AcceptedTimestamp = _timestampService.TimestampFromDateTimeOffset(connectionAcceptedTime),
                    ProfileUrl = prospect.ProfileUrl
                };

                newProspects.Add(newProspect);
            }

            await ProcessNewlyAcceptedProspects(newProspects, message);
        }

        private async Task ProcessNewConnectionsAsync(IList<string> newProspectNames, MonitorForNewAcceptedConnectionsBody message)
        {
            IList<NewProspectConnectionRequest> newProspects = new List<NewProspectConnectionRequest>();
            foreach (string prospectName in newProspectNames)
            {
                NewProspectConnectionRequest newProspect = new()
                {
                    ProspectName = prospectName,
                    AcceptedTimestamp = _timestampService.TimestampNow(),
                    ProfileUrl = ""
                };

                newProspects.Add(newProspect);
            }

            await ProcessNewlyAcceptedProspects(newProspects, message);
        }

        #endregion

        #region MonitorForNewConnections on Notifications Page

        /// <summary>
        /// This method is meant to be executed once on initial page load to check for new user's notifications
        /// </summary>
        /// <param name="webDriver"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private async Task CheckForNewConnectionsOnPageLoad(IWebDriver webDriver, MonitorForNewAcceptedConnectionsBody message)
        {
            IList<NewProspectConnectionRequest> newProspects = _linkedInPageFacade.LinkedInNotificationsPage.GatherAllNewProspectInfo(webDriver, message.TimeZoneId);
            if (newProspects.Count > 0)
            {
                // fire off request to initiate follow up message phase
                await ProcessNewlyAcceptedProspects(newProspects, message);
            }
        }

        /// <summary>
        /// Main method for checking for new notifications on the Notifications page
        /// </summary>
        /// <param name="webDriver"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private async Task LookForNewConnections(IWebDriver webDriver, MonitorForNewAcceptedConnectionsBody message)
        {
            // first grab any new notifications displayed in the view 

            bool areNewNotification = _linkedInPageFacade.LinkedInNavBar.AreNewNotifications(webDriver);            

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
                RequestUrl = $"api/MonitorForNewProspects/{message.HalId}/new-prospects",
                NewAcceptedProspectsConnections = newProspects,
                ApplicationUserId = message.UserId
            };

            await _campaignProcessingPhase.ProcessNewlyAcceptedProspectsAsync(request);
        }


        private IList<NewProspectConnectionRequest> GrabNewlyConnectedProspectsInfo(IWebDriver webDriver, MonitorForNewAcceptedConnectionsBody message)            
        {
            IList<NewProspectConnectionRequest> newProspectInfoRequests = new List<NewProspectConnectionRequest>();

            HalOperationResult<IOperationResponse> result = _linkedInPageFacade.LinkedInNotificationsPage.ClickNewNotificationsButton<IOperationResponse>(webDriver);
            // if the button was not clicked successfully, or it wasn't found
            if(result.Succeeded == false)
            {
                // if web driver failed to locate or click new notifications button, click Notifications tab directly
                result = _linkedInPageFacade.LinkedInNavBar.ClickNotificationsTab<IOperationResponse>(webDriver);
                if (result.Succeeded == false)
                {
                    return newProspectInfoRequests;
                }
            }

            newProspectInfoRequests = _linkedInPageFacade.LinkedInNotificationsPage.GatherAllNewProspectInfo(webDriver, message.TimeZoneId);
            return newProspectInfoRequests;
        }

        private HalOperationResult<T> GoToPage<T>(IWebDriver webDriver, string pageUrl)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            if (webDriver.Url.Contains(pageUrl) == false)
            {
                // first navigate to messages
                result = _linkedInPageFacade.LinkedInHomePage.GoToPage<T>(webDriver, pageUrl);
            }
            else
            {
                result.Succeeded = true;
            }

            return result;
        }

        #endregion
    }
}
