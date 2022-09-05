using OpenQA.Selenium;
using System.Collections.Generic;

namespace Domain.Services.Interfaces.POMs
{
    public interface IMonitorForNewConnectionsServicePOM
    {
        int? GetConnectionsCount(IWebDriver webDriver);
        IList<Models.RecentlyAddedProspect> GetAllRecentlyAdded(IWebDriver webDriver);
    }
}
