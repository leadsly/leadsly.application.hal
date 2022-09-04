using OpenQA.Selenium;

namespace Domain.Interactions.FollowUpMessage.EnterProspectName
{
    public class EnterProspectNameInteraction : InteractionBase
    {
        public IWebDriver WebDriver { get; set; }
        public string ProspectName { get; set; }
    }
}
