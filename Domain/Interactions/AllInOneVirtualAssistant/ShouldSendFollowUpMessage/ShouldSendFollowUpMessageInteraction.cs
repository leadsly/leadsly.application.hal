using OpenQA.Selenium;

namespace Domain.Interactions.AllInOneVirtualAssistant.ShouldSendFollowUpMessage
{
    public class ShouldSendFollowUpMessageInteraction : InteractionBase
    {
        public string PreviousMessageContent { get; set; }
        public string ProspectName { get; set; }
        public IWebElement Prospect { get; set; }
        public string CampaignProspectId { get; set; }
    }
}
