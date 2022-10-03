using OpenQA.Selenium;
using System.Collections.Generic;

namespace Domain.Services.Interfaces.POMs
{
    public interface IScreenHouseKeeperServicePOM
    {
        bool CloseConversation(IWebElement closeButton);
        bool CloseCurrentlyFocusedConversation(IWebElement currentPopUpConversation);
        IReadOnlyCollection<IWebElement> GetAllConversationCardsCloseButtons(IWebDriver webDriver);
    }
}
