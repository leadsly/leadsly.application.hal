using OpenQA.Selenium;
using System.Collections.Generic;

namespace Domain.Services.Interfaces.POMs
{
    public interface ICheckOffHoursNewConnectionsServicePOM
    {
        IList<Models.RecentlyAddedProspect> GetAllRecentlyAddedSince(IWebDriver webDriver, int numOfHoursAgo, string timezoneId);
    }
}
