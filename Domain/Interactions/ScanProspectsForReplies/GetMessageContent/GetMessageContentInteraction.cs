using OpenQA.Selenium;

namespace Domain.Interactions.ScanProspectsForReplies.GetMessageContent
{
    public class GetMessageContentInteraction : InteractionBase
    {
        public IWebDriver WebDriver { get; set; }
        public IWebElement Message { get; set; }
    }
}
