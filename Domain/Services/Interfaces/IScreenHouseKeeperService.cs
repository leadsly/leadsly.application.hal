using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Services.Interfaces
{
    public interface IScreenHouseKeeperService
    {
        bool CloseConversation(IWebElement closeButton);

        IReadOnlyCollection<IWebElement> GetAllConversationCardsCloseButtons(IWebDriver webDriver);
    }
}
