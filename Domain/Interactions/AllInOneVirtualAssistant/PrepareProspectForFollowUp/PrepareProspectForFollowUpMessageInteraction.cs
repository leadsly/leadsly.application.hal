using OpenQA.Selenium;

namespace Domain.Interactions.AllInOneVirtualAssistant.PrepareProspectForFollowUp
{
    public class PrepareProspectForFollowUpMessageInteraction : InteractionBase
    {
        public IWebElement ProspectFromTheHitlist { get; set; }
        public string ProspectName { get; set; }
    }
}
