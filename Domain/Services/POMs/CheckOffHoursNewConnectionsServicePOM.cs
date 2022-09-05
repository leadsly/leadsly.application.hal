using Domain.Models;
using Domain.POMs;
using Domain.Services.Interfaces;
using Domain.Services.Interfaces.POMs;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;

namespace Domain.Services.POMs
{
    public class CheckOffHoursNewConnectionsServicePOM : ICheckOffHoursNewConnectionsServicePOM
    {
        public CheckOffHoursNewConnectionsServicePOM(IConnectionsView connectionView, ILogger<MonitorForNewConnectionsServicePOM> logger)
        {
            _connectionView = connectionView;
            _logger = logger;
        }

        private readonly ILogger<MonitorForNewConnectionsServicePOM> _logger;
        private readonly ITimestampService _timestampService;
        private readonly IConnectionsView _connectionView;

        public IList<RecentlyAddedProspect> GetAllRecentlyAddedSince(IWebDriver webDriver, int numOfHoursAgo, string timezoneId)
        {
            _logger.LogDebug("Getting all recently added prospects since {numOfHoursAgo} hours ago", numOfHoursAgo);
            IList<IWebElement> recentlyAdded = _connectionView.GetRecentlyAdded(webDriver);
            if (recentlyAdded == null)
            {
                _logger.LogWarning("Failed to locate any recently added prospects since {numOfHOursAgo} hours ago", numOfHoursAgo);
                return null;
            }

            IList<RecentlyAddedProspect> prospects = new List<RecentlyAddedProspect>();
            foreach (IWebElement recentlyAddedProspect in recentlyAdded)
            {
                if (AddedBeforeDesiredHoursAgo(recentlyAddedProspect, numOfHoursAgo, out int addedNumOfHoursAgo) == true)
                {
                    string prospectName = _connectionView.GetNameFromLiTag(recentlyAddedProspect);
                    string prospectProfileUrl = _connectionView.GetProfileUrlFromLiTag(recentlyAddedProspect);
                    DateTimeOffset acceptedRequest = _timestampService.GetNowLocalized(timezoneId).AddHours(-addedNumOfHoursAgo);
                    RecentlyAddedProspect prospect = new RecentlyAddedProspect
                    {
                        Name = prospectName,
                        AcceptedRequestTimestamp = acceptedRequest.ToUnixTimeSeconds(),
                        ProfileUrl = prospectProfileUrl
                    };
                    prospects.Add(prospect);
                }
            };

            return prospects;
        }

        private bool AddedBeforeDesiredHoursAgo(IWebElement recentlyAdded, int fromMaxHoursAgo, out int numOfHoursAgo)
        {
            IWebElement timeElement = _connectionView.GetTimeTag(recentlyAdded);
            numOfHoursAgo = 0;

            if (timeElement == null)
            {
                return false;
            }

            string timeTagText = timeElement.Text;
            if (timeTagText == null)
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

            if (result <= fromMaxHoursAgo)
            {
                numOfHoursAgo = result;
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
