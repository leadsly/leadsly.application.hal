using OpenQA.Selenium;

namespace Domain.Interactions.AllInOneVirtualAssistant.EnterFollowUpMessage
{
    public class EnterFollowUpMessageInteraction : InteractionBase
    {
        public string Content { get; set; }
        public int OrderNum { get; set; }
        public IWebElement PopUpConversation { get; set; }
    }
}
