using Leadsly.Application.Model.Campaigns;
using OpenQA.Selenium;
using System.Collections.Generic;

namespace Domain.POMs
{
    public interface IConnectionsView
    {
        int GetConnectionsCount(IWebDriver webDriver);

        IWebElement GetConnectionsHeader(IWebDriver webDriver);

        IList<IWebElement> GetRecentlyAdded(IWebDriver webDriver);
        IWebElement GetTimeTag(IWebElement recentlyAddedProspect);

        string GetNameFromLiTag(IWebElement recentlyAddedProspect);

        string GetProfileUrlFromLiTag(IWebElement recentlyAddedProspect);

        IList<RecentlyAddedProspect> GetAllRecentlyAdded(IWebDriver webDriver);

        IList<RecentlyAddedProspect> GetRecentlyAdded(IWebDriver webDriver, int fromMaxHoursAgo);

        IReadOnlyCollection<IWebElement> GetAllConversationsCloseButtons(IWebDriver webDriver);
    }
}
