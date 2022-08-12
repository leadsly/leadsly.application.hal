using Domain.Facades.Interfaces;
using Domain.Providers.Campaigns.Interfaces;
using Domain.Providers.Interfaces;
using Domain.Services.Interfaces;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.Campaigns.Interfaces;
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

namespace Domain.Providers.Campaigns
{
    public class DeepScanProspectsForRepliesProvider : IDeepScanProspectsForRepliesProvider
    {
        public DeepScanProspectsForRepliesProvider(
            ILogger<DeepScanProspectsForRepliesProvider> logger,
            IWebDriverProvider webDriverProvider,
            ITimestampService timestampService,
            IHumanBehaviorService humanBehaviorService,
            ILinkedInPageFacade linkedInPageFacade)
        {
            _logger = logger;
            _humanBehaviorService = humanBehaviorService;
            _timestampService = timestampService;
            _linkedInPageFacade = linkedInPageFacade;
            _webDriverProvider = webDriverProvider;
        }

        private readonly ITimestampService _timestampService;
        private readonly ILogger<DeepScanProspectsForRepliesProvider> _logger;
        private readonly IHumanBehaviorService _humanBehaviorService;
        private readonly ILinkedInPageFacade _linkedInPageFacade;
        private readonly IWebDriverProvider _webDriverProvider;

        public HalOperationResult<T> ExecutePhase<T>(ScanProspectsForRepliesBody message) where T : IOperationResponse
        {
            HalOperationResult<T> result = SetUpForScanning<T>(message);
            if (result.Succeeded == false)
            {
                return result;
            }

            IWebDriver webDriver = ((IGetOrCreateWebDriverOperation)result.Value).WebDriver;

            return ExecuteScanProspectsForRepliesPhaseOnce<T>(webDriver, message);
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

        private HalOperationResult<T> ExecuteScanProspectsForRepliesPhaseOnce<T>(IWebDriver webDriver, ScanProspectsForRepliesBody message) where T : IOperationResponse
        {
            HalOperationResult<T> result = DeepScanSpecificProspects<T>(webDriver, message);

            return result;
        }

        private int GetVisibleConversationCount(IWebDriver webDriver)
        {
            // stash conversation list items before we hit search
            HalOperationResult<IOperationResponse> visibleItemsResults = _linkedInPageFacade.LinkedInMessagingPage.GetVisibleConversationListItems<IOperationResponse>(webDriver);
            IScrapedHtmlElements elements = visibleItemsResults.Value as IScrapedHtmlElements;
            int conversationListItemCount = elements.HtmlElements.Count;

            return conversationListItemCount;
        }

        private bool WaitForSearchResults(IWebDriver webDriver, int beforeSearchMessagesCount)
        {
            bool searchResultsDiffer = false;
            try
            {
                WebDriverWait waiter = new WebDriverWait(webDriver, TimeSpan.FromSeconds(10));
                waiter.Until((drv) =>
                {
                    HalOperationResult<IOperationResponse> result = _linkedInPageFacade.LinkedInMessagingPage.GetVisibleConversationListItems<IOperationResponse>(webDriver);
                    IScrapedHtmlElements elementsAfterSearch = result.Value as IScrapedHtmlElements;
                    searchResultsDiffer = beforeSearchMessagesCount != elementsAfterSearch.HtmlElements.ToList().Count;
                    return searchResultsDiffer;
                });
            }
            catch (WebDriverTimeoutException ex)
            {
                _logger.LogWarning(ex, "Search results from before hal entered in search term and AFTER hal entered search term are the same! Most cases they should be different");
            }

            return searchResultsDiffer;
        }

        private IWebElement GetProspectsMessageItem(IWebDriver webDriver, string prospectName, int beforeSearchMessagesCount)
        {
            _logger.LogDebug("Locating prospects message container.");

            IWebElement targetProspect = default;

            // attempst to wait for the prospect to be surfaced to the top of the list                
            Stopwatch sw = new Stopwatch();
            sw.Start();
            while (sw.Elapsed.TotalSeconds < 15)
            {
                bool searchResultsDiffer = WaitForSearchResults(webDriver, beforeSearchMessagesCount);
                if (searchResultsDiffer == false)
                {
                    break;
                }

                HalOperationResult<IOperationResponse> result = _linkedInPageFacade.LinkedInMessagingPage.GetVisibleConversationListItems<IOperationResponse>(webDriver);
                IScrapedHtmlElements elementsAfterSearch = result.Value as IScrapedHtmlElements;
                List<IWebElement> conversationListItemsAfterSearch = elementsAfterSearch.HtmlElements.ToList();

                targetProspect = conversationListItemsAfterSearch.FirstOrDefault();
                if (targetProspect != null)
                {
                    if (_linkedInPageFacade.LinkedInMessagingPage.GetProspectNameFromConversationItem(targetProspect) == prospectName)
                        break;
                }

                // check if no messages is displayed if it is break out of the loop
                if (_linkedInPageFacade.LinkedInMessagingPage.IsNoMessagesDisplayed(webDriver) == true)
                {
                    // this prospect isn't found in our messages
                    _logger.LogInformation("{prospectName} is not found in the messages history. This shouldn't happen.", prospectName);
                    break;
                }
            }

            return targetProspect;
        }

        private bool IsConversationListItemActive(IWebDriver webDriver, IWebElement targetProspect)
        {
            bool isActive = false;
            try
            {
                WebDriverWait wait = new WebDriverWait(webDriver, TimeSpan.FromSeconds(15));
                wait.Until(drv =>
                {
                    isActive = _linkedInPageFacade.LinkedInMessagingPage.IsConversationListItemActive(targetProspect);
                    return isActive;
                });
            }
            catch (WebDriverTimeoutException ex)
            {
                _logger.LogWarning(ex, "Expected message list item to be active, but it is not.");
            }

            return isActive;
        }

        private HalOperationResult<T> DeepScanSpecificProspects<T>(IWebDriver webDriver, ScanProspectsForRepliesBody message)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            int conversationsBeforeSearch = GetVisibleConversationCount(webDriver);

            IList<ProspectRepliedRequest> prospectsReplied = new List<ProspectRepliedRequest>();
            foreach (ContactedCampaignProspect contactedCampaignProspect in message.ContactedCampaignProspects)
            {
                IWebElement messagingHeader = _linkedInPageFacade.LinkedInMessagingPage.MessagingHeader(webDriver);
                _humanBehaviorService.RandomClickElement(messagingHeader);
                _humanBehaviorService.RandomWaitMilliSeconds(700, 1250);

                result = _linkedInPageFacade.LinkedInMessagingPage.ClearMessagingSearchCriteria<T>(webDriver);
                if (result.Succeeded == false)
                {
                    return result;
                }

                _humanBehaviorService.RandomWaitMilliSeconds(700, 1100);

                // search for each campaign prospect in the messages search field
                _logger.LogDebug("");
                result = _linkedInPageFacade.LinkedInMessagingPage.EnterSearchMessagesCriteria<T>(webDriver, contactedCampaignProspect.Name);
                if (result.Succeeded == false)
                {
                    continue;
                }

                IWebElement targetProspect = GetProspectsMessageItem(webDriver, contactedCampaignProspect.Name, conversationsBeforeSearch);

                if (targetProspect == null)
                {
                    continue;
                }

                _linkedInPageFacade.LinkedInMessagingPage.ClickConverstaionListItem(targetProspect);
                bool isActive = IsConversationListItemActive(webDriver, targetProspect);
                if (isActive == false)
                {
                    continue;
                }

                // we need to now grab the contents of the conversation history
                result = _linkedInPageFacade.LinkedInMessagingPage.GetMessagesContent<T>(webDriver);
                if (result.Succeeded == false)
                {
                    continue;
                }

                IScrapedHtmlElements messageElements = result.Value as IScrapedHtmlElements;
                List<IWebElement> messages = messageElements.HtmlElements.ToList();
                string targetMessage = contactedCampaignProspect.LastFollowUpMessageContent;

                IWebElement targetMessageElement = messages.Where(m => _linkedInPageFacade.LinkedInMessagingPage.GetMessageContent(m).Contains(targetMessage)).FirstOrDefault();
                if (targetMessageElement == null)
                {
                    continue;
                }

                int targetMessageIndex = messages.IndexOf(targetMessageElement);
                int nextMessageIndex = targetMessageIndex + 1;

                // check if any messages after the target message were prospect's
                for (int i = nextMessageIndex; i < messages.Count; i++)
                {
                    IWebElement nextMessage = messages.ElementAt(i);
                    string prospectName = _linkedInPageFacade.LinkedInMessagingPage.GetProspectNameFromMessageContentPTag(nextMessage);
                    if (prospectName == contactedCampaignProspect.Name)
                    {
                        // we have a resonse from the prospect add it to payload going out to the server
                        string response = _linkedInPageFacade.LinkedInMessagingPage.GetMessageContent(nextMessage);
                        ProspectRepliedRequest request = CreateProspectRepliedRequest(contactedCampaignProspect, response, prospectName, message.TimeZoneId);
                        prospectsReplied.Add(request);
                    }
                }
            }

            result = _linkedInPageFacade.LinkedInMessagingPage.ClearMessagingSearchCriteria<T>(webDriver);
            if (result.Succeeded == false)
            {
                return result;
            }

            IProspectsRepliedPayload payload = new ProspectsRepliedPayload
            {
                ProspectsReplied = prospectsReplied
            };

            result.Value = (T)payload;
            result.Succeeded = true;
            return result;
        }

        private ProspectRepliedRequest CreateProspectRepliedRequest(ContactedCampaignProspect campaignProspect, string responseMessage, string prospectName, string timeZoneId)
        {
            return new()
            {
                ResponseMessageTimestamp = _timestampService.TimestampNow(),
                CampaignProspectId = campaignProspect.CampaignProspectId,
                ResponseMessage = responseMessage,
                ProspectName = prospectName,
                ProspectProfileUrl = string.Empty
            };
        }
    }
}
