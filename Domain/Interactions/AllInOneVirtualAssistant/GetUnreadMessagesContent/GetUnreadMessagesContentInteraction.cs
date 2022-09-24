using OpenQA.Selenium;
using System.Collections.Generic;

namespace Domain.Interactions.AllInOneVirtualAssistant.GetUnreadMessagesContent
{
    public class GetUnreadMessagesContentInteraction : InteractionBase
    {
        public IList<IWebElement> Messages { get; set; }
    }
}
