using Leadsly.Application.Model.Campaigns;
using OpenQA.Selenium;

namespace Domain.Interactions.Networking.ConnectWithProspect
{
    public class ConnectWithProspectInteraction : InteractionBase
    {
        public NetworkingMessageBody Message { get; set; }
        public IWebDriver WebDriver { get; set; }
        public IWebElement Prospect { get; set; }
        public int CurrentPage { get; set; }
        public int TotalResults { get; set; }
    }
}
