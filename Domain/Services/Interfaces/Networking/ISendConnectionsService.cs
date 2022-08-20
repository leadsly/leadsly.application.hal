using Leadsly.Application.Model.Campaigns;
using OpenQA.Selenium;
using System.Collections.Generic;

namespace Domain.Services.Interfaces.Networking
{
    public interface ISendConnectionsService
    {
        bool SendConnections(IWebDriver webDriver, NetworkingMessageBody message, IList<IWebElement> connectableProspects);
    }
}
