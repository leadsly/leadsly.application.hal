using Domain.Facades.Interfaces;
using Domain.POMs.Pages;
using Domain.Providers.Campaigns.Interfaces;
using Domain.Providers.Interfaces;
using Domain.Services.Interfaces;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.Campaigns.Interfaces;
using Leadsly.Application.Model.Entities.Campaigns;
using Leadsly.Application.Model.LinkedInPages.Interface;
using Leadsly.Application.Model.Requests.FromHal;
using Leadsly.Application.Model.Responses;
using Leadsly.Application.Model.WebDriver;
using Leadsly.Application.Model.WebDriver.Interfaces;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Providers.Campaigns
{
    public class ScanProspectsForRepliesProvider : IScanProspectsForRepliesProvider
    {
        public ScanProspectsForRepliesProvider(
            ILogger<ScanProspectsForRepliesProvider> logger,
            IPhaseDataProcessingProvider phaseDataProcessingProvider,
            IWebDriverProvider webDriverProvider,
            ILinkedInPageFacade linkedInPageFacade,
            IHumanBehaviorService humanService,
            ITimestampService timestampService)
        {
            _logger = logger;
            _humanService = humanService;
            _linkedInPageFacade = linkedInPageFacade;
            _webDriverProvider = webDriverProvider;
            _timestampService = timestampService;
            _phaseDataProcessingProvider = phaseDataProcessingProvider;
        }

        private readonly IPhaseDataProcessingProvider _phaseDataProcessingProvider;
        private readonly IHumanBehaviorService _humanService;
        private readonly ILogger<ScanProspectsForRepliesProvider> _logger;
        private readonly ILinkedInPageFacade _linkedInPageFacade;
        private readonly IWebDriverProvider _webDriverProvider;        
        private readonly ITimestampService _timestampService;        
        public static bool IsRunning { get; private set; }

        public async Task<HalOperationResult<T>> ExecutePhaseAsync<T>(ScanProspectsForRepliesBody message) where T : IOperationResponse
        {
            HalOperationResult<T> result = SetUpForScanning<T>(message);
            if(result.Succeeded == false)
            {
                return result;
            }

            IWebDriver webDriver = ((IGetOrCreateWebDriverOperation)result.Value).WebDriver;

            return await ExecutePhaseUntilEndOfWorkDayAsync<T>(webDriver, message);
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

        public async Task<HalOperationResult<T>> ExecutePhaseUntilEndOfWorkDayAsync<T>(IWebDriver webDriver, ScanProspectsForRepliesBody message) where T : IOperationResponse
        {
            HalOperationResult<T> result = new();
            IsRunning = true;
            DateTimeOffset endOfWorkDayInZone = _timestampService.GetDateTimeWithZone(message.TimeZoneId, message.EndWorkTime);
            while (_timestampService.GetDateTimeNowWithZone(message.TimeZoneId) < endOfWorkDayInZone)
            {
                _humanService.RandomWaitSeconds(30, 45);

                await ScanProspectsAsync<T>(webDriver, message);
            }

            _logger.LogInformation("Stopping to scan for prospect replies. ScanProspectsForReplies finished running because it is end of the work day.");
            _webDriverProvider.CloseBrowser<T>(BrowserPurpose.ScanForReplies);

            IsRunning = false;

            result.Succeeded = true;
            return result;
        }

        private async Task<HalOperationResult<T>> ScanProspectsAsync<T>(IWebDriver webDriver, ScanProspectsForRepliesBody message)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            // grab the first batch of 20 conversations
            result = _linkedInPageFacade.LinkedInMessagingPage.GetVisibleConversationListItems<T>(webDriver);
            if(result.Succeeded == false)
            {
                return result;
            }

            // check if any of the list items contain the new message notification
            IScrapedHtmlElements elements = result.Value as IScrapedHtmlElements;

            IList<ProspectRepliedRequest> prospectsReplied = new List<ProspectRepliedRequest>();
            IList<IWebElement> newMessagesListItems = GrabNewMessagesListItems(elements.HtmlElements);
            if(newMessagesListItems.Count > 0)
            {
                // extract those users names and send back to the api server for processing
                foreach (IWebElement prospectMessage in newMessagesListItems)
                {
                    string prospectName = _linkedInPageFacade.LinkedInMessagingPage.GetProspectNameFromConversationItem(prospectMessage);

                    // blank for now because it is hard to get profile url, will add later                
                    _linkedInPageFacade.LinkedInMessagingPage.ClickConverstaionListItem(prospectMessage);

                    bool isActive = WaitUntilConversationListItemIsActive(webDriver, prospectMessage);
                    if(isActive == false)
                    {
                        continue;
                    }

                    _humanService.RandomWaitMilliSeconds(500, 1100);
                    IWebElement messagingHeader = _linkedInPageFacade.LinkedInMessagingPage.MessagingHeader(webDriver);
                    _humanService.RandomClickElement(messagingHeader);

                    // we need to now grab the contents of the conversation history
                    result = _linkedInPageFacade.LinkedInMessagingPage.GetMessagesContent<T>(webDriver);
                    if (result.Succeeded == false)
                    {
                        continue;
                    }

                    IScrapedHtmlElements messageElements = result.Value as IScrapedHtmlElements;
                    List<IWebElement> messages = messageElements.HtmlElements.ToList();
                    IWebElement lastMessage = messages.LastOrDefault();
                    string responseMessage = string.Empty;
                    if(lastMessage != null)
                    {
                        responseMessage = lastMessage.Text;
                    }

                    if(responseMessage != string.Empty)
                    {
                        ProspectRepliedRequest potentialProspectResponse = new ProspectRepliedRequest()
                        {
                            ProspectName = prospectName,
                            ResponseMessage = responseMessage,
                            ResponseMessageTimestamp = _timestampService.TimestampNowWithZone(message.TimeZoneId),
                            CampaignProspectId = "",
                            ProspectProfileUrl = ""
                        };
                        prospectsReplied.Add(potentialProspectResponse);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to retrieve last message from the prospect");
                    }
                    
                }
            }

            if(prospectsReplied.Count > 0)
            {
                // fire off request to the application server for processing of prospects that have replied to us (may not be prospects from our campaigns so we will ignore those)\
                await _phaseDataProcessingProvider.ProcessProspectsRepliedAsync<T>(prospectsReplied, message);
            }

            result.Succeeded = true;
            return result;
        }

        private bool WaitUntilConversationListItemIsActive(IWebDriver webDriver, IWebElement prospectMessage)
        {
            bool active = false;
            try
            {
                WebDriverWait wait = new WebDriverWait(webDriver, TimeSpan.FromSeconds(5));
                wait.Until(drv => _linkedInPageFacade.LinkedInMessagingPage.IsConversationListItemActive(prospectMessage));
                active = true;
            }
            catch (Exception ex)
            {
                int timeout = 5;
                _logger.LogError(ex, "Waited for conversation list item to become active but it never did. Waited for {timeout}", timeout);
            }
            return active;
        }

        private IList<IWebElement> GrabNewMessagesListItems(IReadOnlyCollection<IWebElement> listItems)
        {
            IList<IWebElement> newConversationListItem = new List<IWebElement>();
            foreach (IWebElement listItem in listItems)
            {
                if(_linkedInPageFacade.LinkedInMessagingPage.HasNotification(listItem) == true)
                {
                    newConversationListItem.Add(listItem);
                }
            }

            return newConversationListItem;
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
    }
}
