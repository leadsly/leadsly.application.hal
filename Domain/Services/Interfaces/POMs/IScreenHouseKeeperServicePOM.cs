using OpenQA.Selenium;
using System.Collections.Generic;

namespace Domain.Services.Interfaces.POMs
{
    public interface IScreenHouseKeeperServicePOM
    {
        bool CloseConversation(IWebElement closeButton);

        IReadOnlyCollection<IWebElement> GetAllConversationCardsCloseButtons(IWebDriver webDriver);
    }
}
