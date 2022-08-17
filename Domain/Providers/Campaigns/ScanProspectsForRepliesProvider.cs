using Domain.Facades.Interfaces;
using Domain.Providers.Campaigns.Interfaces;
using Domain.Providers.Interfaces;
using Domain.Services.Interfaces;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
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
using System.Linq;
using System.Threading.Tasks;

namespace Domain.Providers.Campaigns
{
    public class ScanProspectsForRepliesProvider : IScanProspectsForRepliesProvider
    {
        public ScanProspectsForRepliesProvider(
            ILogger<ScanProspectsForRepliesProvider> logger,
            IPhaseDataProcessingProvider phaseDataProcessingProvider,
            IWebDriverProvider webDriverProvider,
            IScreenHouseKeeperService screenHouseKeeperService,
            ILinkedInPageFacade linkedInPageFacade,
            IHumanBehaviorService humanService,
            ITimestampService timestampService)
        {
            _logger = logger;
            _screenHouseKeeperService = screenHouseKeeperService;
            _humanService = humanService;
            _linkedInPageFacade = linkedInPageFacade;
            _webDriverProvider = webDriverProvider;
            _timestampService = timestampService;
            _phaseDataProcessingProvider = phaseDataProcessingProvider;
        }

        private readonly IPhaseDataProcessingProvider _phaseDataProcessingProvider;
        private readonly IHumanBehaviorService _humanService;
        private readonly ILogger<ScanProspectsForRepliesProvider> _logger;
        private readonly IScreenHouseKeeperService _screenHouseKeeperService;
        private readonly ILinkedInPageFacade _linkedInPageFacade;
        private readonly IWebDriverProvider _webDriverProvider;
        private readonly ITimestampService _timestampService;
        public static bool IsRunning { get; private set; }

        public async Task<HalOperationResult<T>> ExecutePhaseAsync<T>(ScanProspectsForRepliesBody message) where T : IOperationResponse
        {
            HalOperationResult<T> result = SetUpForScanning<T>(message);
            if (result.Succeeded == false)
            {
                return result;
            }

            IWebDriver webDriver = ((IGetOrCreateWebDriverOperation)result.Value).WebDriver;

            try
            {
                await ExecutePhaseUntilEndOfWorkDayAsync(webDriver, message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occured while scanning prospects for replies");
                throw;
            }
            finally
            {
                _logger.LogInformation("[ScanProspectsForReplies]: Stopping to scan for prospect replies. ScanProspectsForReplies finished running because it is end of the work day.");
                _webDriverProvider.CloseBrowser<T>(BrowserPurpose.ScanForReplies);
            }

            result.Succeeded = true;
            return result;
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

            HalOperationResult<T> driverOperationResult = _webDriverProvider.GetOrCreateWebDriver<T>(operationData, message.GridNamespaceName, message.GridServiceDiscoveryName);
            if (driverOperationResult.Succeeded == false)
            {
                _logger.LogWarning("There was an issue getting or creating webdriver instance");
                return result;
            }

            IWebDriver webDriver = ((IGetOrCreateWebDriverOperation)driverOperationResult.Value).WebDriver;

            result = GoToPage<T>(webDriver, message.PageUrl);
            if (result.Succeeded == false)
            {
                return result;
            }

            result.Value = driverOperationResult.Value;
            result.Succeeded = true;
            return result;
        }

        public async Task ExecutePhaseUntilEndOfWorkDayAsync(IWebDriver webDriver, ScanProspectsForRepliesBody message)
        {
            _logger.LogDebug("[ScanProspectsForReplies]: Setting IsRunning property to 'true'");
            IsRunning = true;
            try
            {
                DateTimeOffset endOfWorkDayLocal = _timestampService.ParseDateTimeOffsetLocalized(message.TimeZoneId, message.EndOfWorkday);
                while (_timestampService.GetNowLocalized(message.TimeZoneId) < endOfWorkDayLocal)
                {
                    _logger.LogDebug("[ScanProspectsForReplies]: Waiting between 30 to 45 seconds to start scanning.");
                    _humanService.RandomWaitSeconds(30, 45);

                    _logger.LogDebug("[ScanProspectsForReplies]: Looking to close any active message windows.");
                    CloseAllConversations(webDriver);

                    await ScanProspectsAsync<IOperationResponse>(webDriver, message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occured while executing ScanProspectsForReplies phase");
            }
            finally
            {
                _logger.LogDebug("[ScanProspectsForReplies]: Setting IsRunning property to 'false'");
                IsRunning = false;
            }

            _logger.LogInformation("[ScanProspectsForReplies]: Stopping to look for new messages from prospects. ScanProspectsForReplies finished running because it is end of the work day");
        }

        private void CloseAllConversations(IWebDriver webDriver)
        {
            IReadOnlyCollection<IWebElement> closeButtons = _screenHouseKeeperService.GetAllConversationCardsCloseButtons(webDriver);
            foreach (IWebElement closeButton in closeButtons)
            {
                _humanService.RandomWaitSeconds(1, 3);
                _screenHouseKeeperService.CloseConversation(closeButton);
            }
        }

        private async Task<HalOperationResult<T>> ScanProspectsAsync<T>(IWebDriver webDriver, ScanProspectsForRepliesBody message)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            // grab the first batch of 20 conversations
            result = _linkedInPageFacade.LinkedInMessagingPage.GetVisibleConversationListItems<T>(webDriver);
            if (result.Succeeded == false)
            {
                return result;
            }

            // check if any of the list items contain the new message notification
            IScrapedHtmlElements elements = result.Value as IScrapedHtmlElements;

            IList<ProspectRepliedRequest> prospectsReplied = new List<ProspectRepliedRequest>();
            IList<IWebElement> newMessagesListItems = GrabNewMessagesListItems(elements.HtmlElements);
            if (newMessagesListItems.Count > 0)
            {
                // extract those users names and send back to the api server for processing
                foreach (IWebElement prospectMessage in newMessagesListItems)
                {
                    string prospectName = _linkedInPageFacade.LinkedInMessagingPage.GetProspectNameFromConversationItem(prospectMessage);

                    // blank for now because it is hard to get profile url, will add later                
                    _linkedInPageFacade.LinkedInMessagingPage.ClickConverstaionListItem(prospectMessage);

                    bool isActive = WaitUntilConversationListItemIsActive(webDriver, prospectMessage);
                    if (isActive == false)
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
                    if (lastMessage != null)
                    {
                        responseMessage = lastMessage.Text;
                    }

                    if (responseMessage != string.Empty)
                    {
                        ProspectRepliedRequest potentialProspectResponse = new ProspectRepliedRequest()
                        {
                            ProspectName = prospectName,
                            ResponseMessage = responseMessage,
                            ResponseMessageTimestamp = _timestampService.TimestampNow(),
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

            if (prospectsReplied.Count > 0)
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
                if (_linkedInPageFacade.LinkedInMessagingPage.HasNotification(listItem) == true)
                {
                    _logger.LogInformation($"This ListItem [{listItem.Text}] does contain a new notification");
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
