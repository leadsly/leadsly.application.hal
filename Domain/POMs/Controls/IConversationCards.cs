using OpenQA.Selenium;
using System.Collections.Generic;

namespace Domain.POMs.Controls
{
    public interface IConversationCards
    {
        IList<IWebElement> GetAllConversationCloseButtons(IWebDriver webDriver);
        IWebElement GetCloseConversationButton(IWebDriver webDriver, IWebElement conversationPopup);
    }
}
