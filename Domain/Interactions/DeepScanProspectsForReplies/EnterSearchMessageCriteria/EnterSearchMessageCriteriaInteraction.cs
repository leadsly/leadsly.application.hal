using OpenQA.Selenium;

namespace Domain.Interactions.DeepScanProspectsForReplies.EnterSearchMessageCriteria
{
    public class EnterSearchMessageCriteriaInteraction : IInteraction
    {
        public IWebDriver WebDriver { get; set; }
        public string SearchCriteria { get; set; }
    }
}
