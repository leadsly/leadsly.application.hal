using Domain.POMs;
using Domain.POMs.Controls;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PageObjects
{
    public class ConnectionsView : IConnectionsView
    {
        public ConnectionsView(ILogger<ConnectionsView> logger, IConversationCards conversationCards, IWebDriverUtilities webDriverUtilities)
        {
            _logger = logger;
            _webDriverUtilities = webDriverUtilities;
            _conversationCards = conversationCards;
        }

        private readonly ILogger<ConnectionsView> _logger;
        private readonly IWebDriverUtilities _webDriverUtilities;
        private readonly IConversationCards _conversationCards;

        private void WaitUntilUlTagIsVisible(IWebDriver webDriver)
        {
            WebDriverWait wait = new WebDriverWait(webDriver, TimeSpan.FromSeconds(30));
            try
            {
                wait.Until(drv =>
                {
                    IWebElement recentlyAddedUlTag = RecentlyAddedUlTag(drv);
                    return recentlyAddedUlTag != null;
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to locate list of recently added prospects in 30 seconds");
            }
        }
        private IWebElement RecentlyAddedUlTag(IWebDriver webDriver)
        {
            IWebElement recentlyAdded = default;
            try
            {
                recentlyAdded = webDriver.FindElement(By.CssSelector(".scaffold-finite-scroll__content ul"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to locate recently added ul tag that contains all of the recently added connections. Failed to locate by css selector '.scaffold-finite-scroll__content ul'");
            }
            return recentlyAdded;
        }


        private IList<IWebElement> RecentlyAddedLis(IWebDriver webDriver)
        {
            IList<IWebElement> recentlyAddedProspects = default;
            try
            {
                WaitUntilUlTagIsVisible(webDriver);
                recentlyAddedProspects = RecentlyAddedUlTag(webDriver).FindElements(By.TagName("li"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to locate recently added li elements");
            }
            return recentlyAddedProspects;
        }

        public string GetNameFromLiTag(IWebElement liTag)
        {
            string prospectName = string.Empty;
            try
            {
                IWebElement prospectNameSpan = liTag.FindElement(By.ClassName("mn-connection-card__name"));
                if (prospectNameSpan != null)
                {
                    prospectName = prospectNameSpan.Text;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to locate prospect's name from li tag by class name 'mn-connection-card__name'");
            }

            return prospectName;
        }

        public string GetProfileUrlFromLiTag(IWebElement liTag)
        {
            string prospectProfileUrl = string.Empty;
            try
            {
                IWebElement prospectProfileUrlATag = liTag.FindElement(By.ClassName("mn-connection-card__link"));
                if (prospectProfileUrlATag != null)
                {
                    prospectProfileUrl = prospectProfileUrlATag.GetAttribute("href");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to locate prospect's profile url from li tag by class name 'mn-connection-card__link'");
            }

            return prospectProfileUrl;
        }

        public IList<IWebElement> GetRecentlyAdded(IWebDriver webDriver)
        {
            IList<IWebElement> recentlyAdded = _webDriverUtilities.WaitUntilNotNull(RecentlyAddedLis, webDriver, 30);
            return recentlyAdded;
        }

        public IWebElement GetTimeTag(IWebElement webElement)
        {
            IWebElement timeTag = default;
            try
            {
                timeTag = webElement.FindElement(By.ClassName("time-badge"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to locate time badge by class name 'time-badge'");
            }
            return timeTag;
        }

        private IWebElement ConnectionsHeader(IWebDriver webDriver)
        {
            IWebElement connectionsHeader = default;
            try
            {
                connectionsHeader = webDriver.FindElement(By.CssSelector(".mn-connections__header h1"));
            }
            catch (Exception ex)
            {
                // this is already being logged
            }
            return connectionsHeader;
        }

        private void WaitUntilHeaderIsVisible(IWebDriver webDriver)
        {
            WebDriverWait wait = new WebDriverWait(webDriver, TimeSpan.FromSeconds(30));
            try
            {
                wait.Until(drv =>
                {
                    IWebElement header = ConnectionsHeader(webDriver);
                    return header != null;
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to locate connection header after waiting for 30 seconds by class name 'mn-connections__header'");
            }
        }

        public IWebElement GetConnectionsHeader(IWebDriver webDriver)
        {
            IWebElement connectionHeader = _webDriverUtilities.WaitUntilNotNull(ConnectionsHeader, webDriver, 30);
            return connectionHeader;
        }

        public int GetConnectionsCount(IWebDriver webDriver)
        {
            WaitUntilHeaderIsVisible(webDriver);

            IWebElement connectionsHeader = ConnectionsHeader(webDriver);
            if (connectionsHeader == null)
            {
                return -1;
            }

            string header = connectionsHeader.Text;
            if (header == string.Empty)
            {
                return -1;
            }

            string connectionsCount = header.Split().FirstOrDefault();
            if (connectionsCount == string.Empty)
            {
                return -1;
            }

            if (int.TryParse(connectionsCount, out int result) == false)
            {
                return -1;
            }

            return result;
        }

        public IReadOnlyCollection<IWebElement> GetAllConversationsCloseButtons(IWebDriver webDriver)
        {
            return _conversationCards.GetAllConversationCloseButtons(webDriver);
        }

    }
}
