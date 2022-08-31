using OpenQA.Selenium;

namespace Domain.Interactions.DeepScanProspectsForReplies.GetProspectsMessageItem
{
    public class GetProspectsMessageItemInteraction : IInteraction
    {
        public IWebDriver WebDriver { get; set; }
        public string ProspectName { get; set; }
        public int MessagesCountBefore { get; set; }
    }
}
