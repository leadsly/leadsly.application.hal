using OpenQA.Selenium;

namespace Domain.Interactions.DeepScanProspectsForReplies.CheckMessagesHistory
{
    public class CheckMessagesHistoryInteraction : IInteraction
    {
        public IWebDriver WebDriver { get; set; }
        public string TargetMessage { get; set; }
        public string ProspectName { get; set; }
        public string CampaignProspectId { get; set; }
    }
}
