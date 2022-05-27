using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.POMs.Controls
{
    public interface IConversationCards
    {
        IReadOnlyCollection<IWebElement> GetAllConversationCloseButtons(IWebDriver webDriver);
    }
}
