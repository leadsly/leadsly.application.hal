﻿using OpenQA.Selenium;
using System.Collections.Generic;

namespace Domain.Services.Interfaces.POMs
{
    public interface IDeepScanProspectsService
    {
        int GetVisibleConversationCount(IWebDriver webDriver);

        bool ClearMessagingSearchCriteria(IWebDriver webDriver);

        bool EnterSearchMessagesCriteria(IWebDriver webDriver, string searchCriteria);

        IWebElement GetProspectsMessageItem(IWebDriver webDriver, string prospectName, int beforeSearchMessagesCount);
        bool ClickNewMessage(IWebElement newMessage, IWebDriver webDriver);
        IList<IWebElement> GetMessageContents(IWebDriver webDriver);
        string GetProspectNameFromMessageContent(IWebElement messageContent);

    }
}
