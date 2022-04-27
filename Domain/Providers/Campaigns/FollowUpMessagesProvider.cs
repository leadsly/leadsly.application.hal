using Domain.Facades.Interfaces;
using Domain.POMs.Pages;
using Domain.Providers.Campaigns.Interfaces;
using Domain.Providers.Interfaces;
using Domain.Services.Interfaces;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.Campaigns.Interfaces;
using Leadsly.Application.Model.Responses;
using Leadsly.Application.Model.WebDriver;
using Leadsly.Application.Model.WebDriver.Interfaces;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;

namespace Domain.Providers.Campaigns
{
    public class FollowUpMessagesProvider : IFollowUpMessagesProvider
    {
        public FollowUpMessagesProvider(
            ILinkedInPageFacade linkedInPageFacade, 
            ILogger<FollowUpMessagesProvider> logger,
            IHumanBehaviorService humanBehaviorService,
            ITimestampService timestampService,
            IWebDriverProvider webDriverProvider)
        {
            _logger = logger;
            _humanBehaviorService = humanBehaviorService;
            _linkedInPageFacade = linkedInPageFacade;
            _timestampService = timestampService;
            _webDriverProvider = webDriverProvider;            
        }

        private readonly IHumanBehaviorService _humanBehaviorService;
        private readonly IWebDriverProvider _webDriverProvider;
        private readonly ILogger<FollowUpMessagesProvider> _logger;
        private readonly ITimestampService _timestampService;
        private readonly ILinkedInPageFacade _linkedInPageFacade;

        public HalOperationResult<T> ExecutePhase<T>(FollowUpMessageBody message)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            // assume user is authenticated
            WebDriverOperationData operationData = new()
            {
                BrowserPurpose = BrowserPurpose.FollowUpMessages,
                ChromeProfileName = message.ChromeProfileName,
                PageUrl = message.PageUrl
            };

            HalOperationResult<T> driverOperationResult = _webDriverProvider.GetOrCreateWebDriver<T>(operationData);

            if(driverOperationResult.Succeeded == false)
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

            return SendFollowUpMessage<T>(webDriver, message);
        }

        private HalOperationResult<T> SendFollowUpMessage<T>(IWebDriver webDriver, FollowUpMessageBody message)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            result = _linkedInPageFacade.LinkedInMessagingPage.ClickCreateNewMessage<T>(webDriver);
            if(result.Succeeded == false)
            {
                _logger.LogWarning("Failed to click create new message button");
                return result;
            }

            result = _linkedInPageFacade.LinkedInMessagingPage.EnterProspectsName<T>(webDriver, message.ProspectName);
            if(result.Succeeded == false)
            {
                _logger.LogWarning("Failed to enter user's name when creating new message");
                return result;
            }

            result = _linkedInPageFacade.LinkedInMessagingPage.ConfirmProspectName<T>(webDriver);
            if(result.Succeeded == false)
            {
                return result;
            }

            _humanBehaviorService.RandomWaitMilliSeconds(300, 500);

            result = _linkedInPageFacade.LinkedInMessagingPage.ClickWriteAMessageBox<T>(webDriver);
            if(result.Succeeded == false)
            {
                return result;
            }

            _humanBehaviorService.RandomWaitMilliSeconds(300, 500);

            result = _linkedInPageFacade.LinkedInMessagingPage.EnterMessageContent<T>(webDriver, message.Content);
            if(result.Succeeded == false)
            {
                _logger.LogWarning("Failed to enter message content");
                return result;
            }

            result = _linkedInPageFacade.LinkedInMessagingPage.ClickSend<T>(webDriver);
            if(result.Succeeded == false)
            {
                return result;
            }

            IFollowUpMessagePayload messageSent = new FollowUpMessagePayload
            {
                FollowUpMessageSentRequest = new()
                {
                    HalId = message.HalId,
                    CampaignProspectId = message.CampaignProspectId,
                    MessageSentTimestamp = _timestampService.TimestampNowWithZone(message.TimeZoneId),
                    ProspectName = message.ProspectName,
                    MessageOrderNum = message.OrderNum
                }
            };

            result.Value = (T)messageSent;
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
