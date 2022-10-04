using OpenQA.Selenium;
using System.Collections.Generic;

namespace Domain.Services.Interfaces.POMs
{
    public interface IScreenHouseKeeperServicePOM
    {
        bool CloseConversation(IWebElement closeButton);
        bool CloseCurrentlyFocusedConversation(IWebDriver webDRiver, IWebElement currentPopUpConversation);
        IList<IWebElement> GetAllConversationCardsCloseButtons(IWebDriver webDriver);
    }
}
