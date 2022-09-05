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
        IReadOnlyCollection<IWebElement> GetAllConversationsCloseButtons(IWebDriver webDriver);
    }
}
