using OpenQA.Selenium;
using System.Collections.Generic;

namespace Domain.Services.Interfaces.POMs
{
    public interface IScanProspectsServicePOM
    {
        IList<IWebElement> GetNewMessages(IWebDriver webDriver);
        string ProspectNameFromMessage(IWebElement element);
        bool ClickNewMessage(IWebElement newMessage, IWebDriver webDriver);
        IList<IWebElement> GetMessageContent(IWebDriver webDriver);
        void WaitAndRelaxSome();
    }
}
