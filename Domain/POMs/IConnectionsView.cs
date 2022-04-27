using Leadsly.Application.Model.Campaigns;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.POMs
{
    public interface IConnectionsView
    {
        int GetConnectionsCount(IWebDriver webDriver);

        IList<RecentlyAddedProspect> GetAllRecentlyAdded(IWebDriver webDriver);

        IList<RecentlyAddedProspect> GetRecentlyAdded(IWebDriver webDriver, int fromMaxHoursAgo);
    }
}
