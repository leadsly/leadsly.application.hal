using Domain.POMs.Pages;
using Domain.Providers.Campaigns.Interfaces;
using Domain.Providers.Interfaces;
using Domain.Services.Interfaces;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.Responses;
using Leadsly.Application.Model.WebDriver;
using Leadsly.Application.Model.WebDriver.Interfaces;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Providers.Campaigns
{
    public class ScanProspectsForRepliesProvider : IScanProspectsForRepliesProvider
    {
        public ScanProspectsForRepliesProvider(
            ILogger<ScanProspectsForRepliesProvider> logger,
            IWebDriverProvider webDriverProvider,
            ILinkedInHomePage linkedInHomePage,
            ITimestampService timestampService,
            ILinkedInMessagingPage linkedInMessagingPage,
            ILinkedInSearchPage linkedInSearchPage)
        {
            _logger = logger;
            _linkedInMessagingPage = linkedInMessagingPage;
            _webDriverProvider = webDriverProvider;
            _timestampService = timestampService;
            _linkedInSearchPage = linkedInSearchPage;
            _linkedInHomePage = linkedInHomePage;
        }

        private readonly ILogger<ScanProspectsForRepliesProvider> _logger;
        private readonly ILinkedInMessagingPage _linkedInMessagingPage;
        private readonly IWebDriverProvider _webDriverProvider;
        private readonly ILinkedInSearchPage _linkedInSearchPage;
        private readonly ILinkedInHomePage _linkedInHomePage;
        private readonly ITimestampService _timestampService;
        private readonly ICampaignPhaseProcessingService _campaignProcessingPhase;

        public HalOperationResult<T> ExecutePhase<T>(ScanProspectsForRepliesBody message) where T : IOperationResponse
        {
            HalOperationResult<T> result = SetUpForScanning<T>(message);
            if(result.Succeeded == false)
            {
                return result;
            }

            IWebDriver webDriver = ((IGetOrCreateWebDriverOperation)result.Value).WebDriver;

            return ExecutePhaseUntilEndOfWorkDay<T>(webDriver, message);
        }

        public HalOperationResult<T> ExecutePhaseOnce<T>(ScanProspectsForRepliesBody message) where T : IOperationResponse
        {
            HalOperationResult<T> result = SetUpForScanning<T>(message);
            if (result.Succeeded == false)
            {
                return result;
            }

            IWebDriver webDriver = ((IGetOrCreateWebDriverOperation)result.Value).WebDriver;

            return ExecutePhaseOnce<T>(webDriver, message);
        }

        private HalOperationResult<T> SetUpForScanning<T>(ScanProspectsForRepliesBody message) where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            // assume user is authenticated
            WebDriverOperationData operationData = new()
            {
                BrowserPurpose = BrowserPurpose.ScanForReplies,
                ChromeProfileName = message.ChromeProfileName
            };

            HalOperationResult<T> driverOperationResult = _webDriverProvider.GetOrCreateWebDriver<T>(operationData);
            if (driverOperationResult.Succeeded == false)
            {
                _logger.LogWarning("There was an issue getting or creating webdriver instance");
                return result;
            }

            IWebDriver webDriver = ((IGetOrCreateWebDriverOperation)driverOperationResult.Value).WebDriver;

            result = _webDriverProvider.NewTab<T>(webDriver);
            if (result.Succeeded == false)
            {
                return result;
            }

            result = GoToPage<T>(webDriver, message.PageUrl);
            if (result.Succeeded == false)
            {
                return result;
            }

            result.Value = driverOperationResult.Value;
            result.Succeeded = true;
            return result;
        }

        private HalOperationResult<T> ExecutePhaseOnce<T>(IWebDriver webDriver, ScanProspectsForRepliesBody message) where T : IOperationResponse
        {
            await ScanProspects(webDriver, message);
        }

        public HalOperationResult<T> ExecutePhaseUntilEndOfWorkDay<T>(IWebDriver webDriver, ScanProspectsForRepliesBody message) where T : IOperationResponse
        {
            DateTimeOffset endOfWorkDayInZone = DateTimeOffset.FromUnixTimeSeconds(message.EndWorkTime);
            while (_timestampService.GetDateTimeNowWithZone(message.TimeZoneId) < endOfWorkDayInZone)
            {
                await ScanProspects(webDriver, message);
            }
        }

        private HalOperationResult<T> ScanProspects<T>(IWebDriver webDriver, ScanProspectsForRepliesBody message)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new();
                       

            try
            {
                await MonitorForNewConnections(webDriver, message);
            }
            catch (Exception ex)
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
