using Domain.Models.Networking;
using Leadsly.Application.Model.Campaigns;
using OpenQA.Selenium;

namespace Domain.Interactions.Networking.GatherProspects
{
    public class GatherProspectsInteraction : IInteraction
    {
        public NetworkingMessageBody Message { get; set; }
        public IWebDriver WebDriver { get; set; }
    }
}
