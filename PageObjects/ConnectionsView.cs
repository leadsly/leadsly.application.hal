using Domain.POMs;
using Leadsly.Application.Model.Campaigns;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PageObjects
{
    public class ConnectionsView : IConnectionsView
    {
        public ConnectionsView(ILogger<ConnectionsView> logger)
        {
            _logger = logger;
        }

        private readonly ILogger<ConnectionsView> _logger;


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


        private IReadOnlyCollection<IWebElement> RecentlyAddedLis(IWebDriver webDriver)
        {
            IReadOnlyCollection<IWebElement> recentlyAddedProspects = default;
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

        private string GetNameFromLiTag(IWebElement liTag)
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

        private string GetProfileUrlFromLiTag(IWebElement liTag)
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

        public IList<RecentlyAddedProspect> GetAllRecentlyAdded(IWebDriver webDriver)
        {
            IReadOnlyCollection<IWebElement> recentlyAdded = RecentlyAddedLis(webDriver);
            IList<RecentlyAddedProspect> prospects = CreateRecentlyAddedFromElements(recentlyAdded.ToList());
            
            return prospects;
        }

        private IList<RecentlyAddedProspect> CreateRecentlyAddedFromElements(IList<IWebElement> recentlyAdded)
        {
            IList<RecentlyAddedProspect> prospects = new List<RecentlyAddedProspect>();
            foreach (IWebElement li in recentlyAdded)
            {
                string prospectName = GetNameFromLiTag(li);
                string prospectProfileUrl = GetProfileUrlFromLiTag(li);
                RecentlyAddedProspect potentialProspect = new()
                {
                    Name = prospectName,
                    ProfileUrl = prospectProfileUrl
                };

                prospects.Add(potentialProspect);
            }

            return prospects;
        }

        public IList<RecentlyAddedProspect> GetRecentlyAdded(IWebDriver webDriver, int fromMaxHoursAgo)
        {
            IList<IWebElement> recentlyAdded = RecentlyAddedLis(webDriver).ToList();
            IList<IWebElement> added = recentlyAdded.Where(p => AddedBeforeDesiredHoursAgo(p, fromMaxHoursAgo) == true).ToList();

            IList<RecentlyAddedProspect> prospects = CreateRecentlyAddedFromElements(added);

            return prospects;
        }

        private bool AddedBeforeDesiredHoursAgo(IWebElement recentlyAdded, int fromMaxHoursAgo)
        {
            IWebElement timeElement = TimeTag(recentlyAdded);
            if(timeElement == null) 
            {
                return false;
            }

            string timeTagText = timeElement.Text;
            if(timeTagText == null)
            {
                return false;
            }

            if (timeTagText.Contains("minute"))
            {
                return true;
            }

            if (timeTagText.Contains("day"))
            {
                return false;
            }

            if (timeTagText.Contains("week"))
            {
                return false;
            }

            if (timeTagText.Contains("month"))
            {
                return false;
            }

            string resultAsString = string.Empty;
            int result = 0;
            for (int i = 0; i < timeTagText.Length; i++)
            {
                if (Char.IsDigit(timeTagText[i]))
                    resultAsString += timeTagText[i];
            }

            if (resultAsString.Length > 0)
            {
                result = int.Parse(resultAsString);
            }

            if(result <= fromMaxHoursAgo)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private IWebElement TimeTag(IWebElement webElement)
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
                _logger.LogError(ex, "Failed to locate connections header which contains number of connections user has by class name 'mn-connections__header'");
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
                _logger.LogError(ex, "Failed to locate connection header after waiting for 30 seconds");
            }
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

    }
}
