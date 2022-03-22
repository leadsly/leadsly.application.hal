using Domain.POMs.Pages;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Responses;
using Leadsly.Application.Model.Responses.Hal;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PageObjects.Pages
{
    public class LinkedInMessagingPage : ILinkedInMessagingPage
    {
        public LinkedInMessagingPage(ILogger<LinkedInMessagingPage> logger)
        {
            _logger = logger;
        }

        private readonly ILogger<LinkedInMessagingPage> _logger;



        public HalOperationResult<T> ClickCreateNewMessage<T>(IWebDriver webDriver) where T : IOperationResponse
        {
            throw new NotImplementedException();
        }

        public HalOperationResult<T> ClickSend<T>(IWebDriver webDriver) where T : IOperationResponse
        {
            throw new NotImplementedException();
        }

        public HalOperationResult<T> EnterMessageContent<T>(IWebDriver webDriver, string messageContent) where T : IOperationResponse
        {
            throw new NotImplementedException();
        }

        public HalOperationResult<T> EnterProspectsName<T>(IWebDriver webDriver, string name) where T : IOperationResponse
        {
            throw new NotImplementedException();
        }
    }
}
