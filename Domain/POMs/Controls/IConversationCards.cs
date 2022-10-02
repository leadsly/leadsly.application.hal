using OpenQA.Selenium;
using System.Collections.Generic;

namespace Domain.POMs.Controls
{
    public interface IConversationCards
    {
        IReadOnlyCollection<IWebElement> GetAllConversationCloseButtons(IWebDriver webDriver);
        IWebElement GetCloseConversationButton(IWebElement conversationPopup);
    }
}
