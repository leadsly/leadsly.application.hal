using Domain.Pages;
using Domain.Providers.Campaigns.Interfaces;
using Domain.Providers.Interfaces;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.Responses;
using Leadsly.Application.Model.WebDriver;
using Leadsly.Application.Model.WebDriver.Interfaces;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Providers.Campaigns
{
    public class FollowUpMessagesProvider : IFollowUpMessagesProvider
    {
        public FollowUpMessagesProvider(ILinkedInMessagingPage linkedInMessagingPage, ILinkedInHomePage linkedInHomePage, ILogger<FollowUpMessagesProvider> logger, IWebDriverProvider webDriverProvider)
        {
            _logger = logger;
            _linkedInHomePage = linkedInHomePage;
            _webDriverProvider = webDriverProvider;
            _linkedInMessagingPage = linkedInMessagingPage;
        }

        private readonly IWebDriverProvider _webDriverProvider;
        private readonly ILogger<FollowUpMessagesProvider> _logger;
        private readonly ILinkedInHomePage _linkedInHomePage;
        private readonly ILinkedInMessagingPage _linkedInMessagingPage;

        public HalOperationResult<T> ExecutePhase<T>(FollowUpMessagesBody message)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            // assume user is authenticated
            WebDriverOperationData operationData = new()
            {
                BrowserPurpose = BrowserPurpose.FollowUpMessages,
                ChromeProfileName = message.ChromeProfileName,
                RequestedWindowHandleId = message.RequestedWindowHandleId,
                PageUrl = message.PageUrl
            };

            HalOperationResult<IGetOrCreateWebDriverOperation> driverOperationResult = _webDriverProvider.CreateOrGetWebDriver<IGetOrCreateWebDriverOperation>(operationData);

            if(driverOperationResult.Succeeded == false)
            {
                _logger.LogWarning("There was an issue getting or creating webdriver instance");
                return result;
            }

            IWebDriver webDriver = driverOperationResult.Value.WebDriver;

            return SendFollowUpMessages<T>(webDriver, message);
        }

        private HalOperationResult<T> SendFollowUpMessages<T>(IWebDriver webDriver, FollowUpMessagesBody message)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            // first navigate to messages
            result = GoToPage<T>(webDriver, message.PageUrl);
            if(result.Succeeded == false)
            {
                string pageUrl = message.PageUrl;
                _logger.LogError("Failed to navigate to {pageUrl}", pageUrl);
                return result;
            }

            // for each prospect in the list navigate to the messages section on the page and send a message
            foreach (var prospectEntity in message.Data.ProspectEntities)
            {
                string messageContent = message.Data.ProspectsMessages[prospectEntity.Id];
                result = SendFollowUpMessage<T>(webDriver, prospectEntity.Name, messageContent);

                if(result.Succeeded == false)
                {
                    // log the exception and try another prospect
                    string name = prospectEntity.Name;
                    _logger.LogWarning("Failed to send follow up message to {name}", name);
                }
            }

            result.Succeeded = true;
            return result;
        }

        private HalOperationResult<T> SendFollowUpMessage<T>(IWebDriver webDriver, string prospectName, string messageContent)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            result = _linkedInMessagingPage.ClickCreateNewMessage<T>(webDriver);
            if(result.Succeeded == false)
            {
                _logger.LogWarning("Failed to click create new message button");
                return result;
            }

            result = _linkedInMessagingPage.EnterProspectsName<T>(webDriver, prospectName);
            if(result.Succeeded == false)
            {
                _logger.LogWarning("Failed to enter user's name when creating new message");
                return result;
            }

            result = _linkedInMessagingPage.EnterMessageContent<T>(webDriver, messageContent);
            if(result.Succeeded == false)
            {
                _logger.LogWarning("Failed to enter message content");
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
