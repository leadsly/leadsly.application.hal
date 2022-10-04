using OpenQA.Selenium;
using System.Collections.Generic;

namespace Domain.POMs
{
    public interface IConnectionsView
    {
        int GetConnectionsCount(IWebDriver webDriver);
        IWebElement GetConnectionsHeader(IWebDriver webDriver);
        IList<IWebElement> GetRecentlyAdded(IWebDriver webDriver);
        IList<IWebElement> GetRecentlyAddedFiltered(IWebDriver webDriver);
        IWebElement GetTimeTag(IWebElement recentlyAddedProspect);
        string GetNameFromLiTag(IWebElement recentlyAddedProspect);
        string GetNameFromLiTag(IWebDriver webDriver);
        string GetProfileUrlFromLiTag(IWebElement recentlyAddedProspect);
        IList<IWebElement> GetAllConversationsCloseButtons(IWebDriver webDriver);
        bool ClickMessage(IWebElement prospect);
        IWebElement GetProspectSearchInputField(IWebDriver webDriver);
        bool ClickProspectSearchInputField(IWebDriver webDriver);
        IWebElement GetElipsesButton(IWebElement prospect);
        RecentlyAddedResults DetermineRecentlyAddedResults(IWebDriver webDriver);
    }
}
