using OpenQA.Selenium;

namespace Domain.Interactions.DeepScanProspectsForReplies.EnterSearchMessageCriteria
{
    public class EnterSearchMessageCriteriaInteraction : InteractionBase
    {
        public IWebDriver WebDriver { get; set; }
        public string SearchCriteria { get; set; }
    }
}
