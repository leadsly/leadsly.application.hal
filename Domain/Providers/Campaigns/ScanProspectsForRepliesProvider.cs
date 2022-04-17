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
            IWebDriverProvider webDriverProvider,
            ILinkedInPageFacade linkedInPageFacade,
            ITimestampService timestampService)
        {
            _logger = logger;
            _linkedInPageFacade = linkedInPageFacade;
            _webDriverProvider = webDriverProvider;
            _timestampService = timestampService;            
        }

        private readonly ILogger<ScanProspectsForRepliesProvider> _logger;
        private readonly ILinkedInPageFacade _linkedInPageFacade;
        private readonly IWebDriverProvider _webDriverProvider;        
        private readonly ITimestampService _timestampService;
        private readonly ICampaignPhaseProcessingService _campaignProcessingPhase;

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

        public HalOperationResult<T> ExecutePhaseOnce<T>(ScanProspectsForRepliesBody message) where T : IOperationResponse
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

        private HalOperationResult<T> ExecuteScanProspectsForRepliesPhaseOnce<T>(IWebDriver webDriver, ScanProspectsForRepliesBody message) where T : IOperationResponse
        {
            return DeepScanSpecificProspects<T>(webDriver, message);
        }

        public async Task<HalOperationResult<T>> ExecutePhaseUntilEndOfWorkDayAsync<T>(IWebDriver webDriver, ScanProspectsForRepliesBody message) where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            DateTimeOffset endOfWorkDayInZone = DateTimeOffset.FromUnixTimeSeconds(message.EndWorkTime);
            while (_timestampService.GetDateTimeNowWithZone(message.TimeZoneId) < endOfWorkDayInZone)
            {
                await ScanProspectsAsync<T>(webDriver, message);
            }

            result.Succeeded = true;
            return result;
        }

        private HalOperationResult<T> DeepScanSpecificProspects<T>(IWebDriver webDriver, ScanProspectsForRepliesBody message)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            IList<ProspectRepliedRequest> prospectsReplied = new List<ProspectRepliedRequest>();
            foreach (CampaignProspect campaignProspect in message.ContactedCampaignProspects)
            {
                result = _linkedInPageFacade.LinkedInMessagingPage.ClearMessagingSearchCriteria<T>(webDriver);
                if (result.Succeeded == false)
                {
                    return result;
                }

                // search for each campaign prospect in the messages search field
                result = _linkedInPageFacade.LinkedInMessagingPage.EnterSearchMessagesCriteria<T>(webDriver, campaignProspect.Name);
                if(result.Succeeded == false)
                {
                    continue;
                }

                // attempst to wait for the prospect to be surfaced to the top of the list
                bool expectedProspectOnTopOfList = false;
                Stopwatch sw = new Stopwatch();
                sw.Start();
                IWebElement targetProspect = default;
                while (expectedProspectOnTopOfList == false || sw.Elapsed.TotalSeconds < 30)
                {
                    result = _linkedInPageFacade.LinkedInMessagingPage.GetVisibleConversationListItems<T>(webDriver);
                    IScrapedHtmlElements elements = result.Value as IScrapedHtmlElements;
                    List<IWebElement> conversationListItems = elements.HtmlElements.ToList();
                    targetProspect = conversationListItems.FirstOrDefault();
                    if(targetProspect != null)
                    {
                        expectedProspectOnTopOfList = _linkedInPageFacade.LinkedInMessagingPage.GetProspectNameFromConversationItem(targetProspect) == campaignProspect.Name;
                    }
                }

                 _linkedInPageFacade.LinkedInMessagingPage.ClickConverstaionListItem(targetProspect);
                WebDriverWait wait = new WebDriverWait(webDriver, TimeSpan.FromSeconds(30));
                bool isActive = wait.Until(drv => _linkedInPageFacade.LinkedInMessagingPage.IsConversationListItemActive(targetProspect));
                if(isActive == false)
                {
                    continue;
                }

                // we need to now grab the contents of the conversation history
                result = _linkedInPageFacade.LinkedInMessagingPage.GetMessagesContent<T>(webDriver);
                if(result.Succeeded == false)
                {
                    continue;
                }

                IScrapedHtmlElements messageElements = result.Value as IScrapedHtmlElements;
                List<IWebElement> messages = messageElements.HtmlElements.ToList();
                string targetMessage = campaignProspect.FollowUpMessage.Content;

                IWebElement targetMessageElement = messages.Where(m => _linkedInPageFacade.LinkedInMessagingPage.GetMessageContent(m).Contains(targetMessage)).FirstOrDefault();
                if(targetMessageElement == null)
                {
                    continue;
                }

                int targetMessageIndex = messages.IndexOf(targetMessageElement);
                int nextMessageIndex = targetMessageIndex + 1;

                // check if any messages after the target message were prospect's
                for (int i = nextMessageIndex; i < messages.Count; i++)
                {
                    IWebElement nextMessage = messages.ElementAt(i);
                    if (_linkedInPageFacade.LinkedInMessagingPage.GetProspectNameFromMessageDetailDiv(nextMessage) == campaignProspect.Name)
                    {
                        // we have a resonse from the prospect add it to payload going out to the server
                        string response = _linkedInPageFacade.LinkedInMessagingPage.GetMessageContent(nextMessage);
                        ProspectRepliedRequest request = CreateProspectRepliedRequest(campaignProspect, response);
                        prospectsReplied.Add(request);
                    }
                }
            }

            result = _linkedInPageFacade.LinkedInMessagingPage.ClearMessagingSearchCriteria<T>(webDriver);
            if (result.Succeeded == false)
            {
                return result;
            }

            // grab the first batch of 20 conversations
            //result = _linkedInPageFacade.LinkedInMessagingPage.GetVisibleConversationListItems<T>(webDriver);
            //if (result.Succeeded == false)
            //{
            //    return result;
            //}

            //IScrapedHtmlElements elements = result.Value as IScrapedHtmlElements;            
            //List<IWebElement> conversationListItems = elements.HtmlElements.ToList();

            //// this is the list of the prospects as IWebElement whom we've contacted in the last 24 hours
            //List<IWebElement> contactedProspects = new List<IWebElement>();
            //foreach (CampaignProspect contacted in message.ContactedCampaignProspects)
            //{
            //    IWebElement contactedProspect = conversationListItems.Where(c => _linkedInPageFacade.LinkedInMessagingPage.GetProspectNameFromConversationItem(c) == contacted.Name).Single();
            //    contactedProspects.Add(contactedProspect);
            //}

            //// the list is now filtered down to the prospects that we've sent a connection to in the last 24 hours
            //// we have to go through all of those prospects messages regardless if they have a notification badge or not
            //// [Jordan Sprague: 4/15/2022]: What if the leadsly user manually responds before we check for prospect responses? Then it wouldn't be seen as a new incoming message
            //foreach (IWebElement contactedProspect in contactedProspects)
            //{
            //    //click on the prospect first to make it active and bring the conversation history in the right panel
            //    _linkedInPageFacade.LinkedInMessagingPage.ClickConverstaionListItem(contactedProspect);
            //    if(_linkedInPageFacade.LinkedInMessagingPage.IsConversationListItemActive(contactedProspect) == false)
            //    {
            //        string prospectName = _linkedInPageFacade.LinkedInMessagingPage.GetProspectNameFromConversationItem(contactedProspect);
            //        _logger.LogInformation("ConversationListItem is not active. It does not have active class. Skipping prospect {prospectName}", prospectName);
            //    }
            //}

            IProspectsRepliedPayload payload = new ProspectsRepliedPayload
            {
                ProspectsReplied = prospectsReplied
            };

            result.Value = (T)payload;
            result.Succeeded = true;
            return result;
        }

        private ProspectRepliedRequest CreateProspectRepliedRequest(CampaignProspect campaignProspect, string responseMessage) 
        {
            return new()
            {
                CampaignProspectId = campaignProspect.CampaignProspectId,
                ResponseMessage = responseMessage
            };
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
    }
}
