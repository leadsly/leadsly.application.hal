using OpenQA.Selenium;

namespace Domain.Interactions.FollowUpMessage.EnterMessage
{
    public class EnterMessageInteraction : InteractionBase
    {
        public IWebDriver WebDriver { get; set; }
        public string Content { get; set; }
        public int OrderNum { get; set; }
    }
}
