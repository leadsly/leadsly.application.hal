using Domain.MQ.Messages;
using OpenQA.Selenium;

namespace Domain.Interactions.Networking.GatherProspects
{
    public class GatherProspectsInteraction : InteractionBase
    {
        public NetworkingMessageBody Message { get; set; }
        public IWebDriver WebDriver { get; set; }
    }
}
