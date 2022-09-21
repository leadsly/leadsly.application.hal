using OpenQA.Selenium;
using System.Collections.Generic;

namespace Domain.Interactions.AllInOneVirtualAssistant.GetMessageContent
{
    public class GetMessagesContentInteraction : InteractionBase
    {
        public IList<IWebElement> Messages { get; set; }
    }
}
