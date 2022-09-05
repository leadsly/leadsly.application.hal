using Domain.Interactions.ScanProspectsForReplies.GetNewMessages.Interfaces;
using Domain.Services.Interfaces.POMs;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using System.Collections.Generic;

namespace Domain.Interactions.ScanProspectsForReplies.GetNewMessages
{
    public class GetNewMessagesInteractionHandler : IGetNewMessagesInteractionHandler
    {
        public GetNewMessagesInteractionHandler(ILogger<GetNewMessagesInteractionHandler> logger, IScanProspectsServicePOM service)
        {
            _logger = logger;
            _service = service;
        }

        private readonly ILogger<GetNewMessagesInteractionHandler> _logger;
        private readonly IScanProspectsServicePOM _service;
        private IList<IWebElement> NewMessages { get; set; } = new List<IWebElement>();

        public bool HandleInteraction(InteractionBase interaction)
        {
            GetNewMessagesInteraction getNewMessagesIntraction = interaction as GetNewMessagesInteraction;
            _service.WaitAndRelaxSome();

            _logger.LogInformation("Scanning prospects for replies interaction.");
            IWebDriver webDriver = getNewMessagesIntraction.WebDriver;
            IList<IWebElement> newMessages = _service.GetNewMessages(webDriver);
            if (newMessages == null)
            {
                return false;
            }
            NewMessages = newMessages;
            return true;
        }

        public IList<IWebElement> GetNewMessages()
        {
            IList<IWebElement> newMessages = NewMessages;
            NewMessages = new List<IWebElement>();
            return newMessages;
        }
    }
}
