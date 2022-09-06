using Domain.Models.MonitorForNewProspects;
using Domain.POMs;
using Domain.Services.Interfaces.POMs;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using System.Collections.Generic;
using System.Linq;

namespace Domain.Services.POMs
{
    public class MonitorForNewConnectionsServicePOM : IMonitorForNewConnectionsServicePOM
    {
        public MonitorForNewConnectionsServicePOM(IConnectionsView connectionView, ILogger<MonitorForNewConnectionsServicePOM> logger)
        {
            _connectionView = connectionView;
            _logger = logger;
        }

        private readonly ILogger<MonitorForNewConnectionsServicePOM> _logger;
        private readonly IConnectionsView _connectionView;

        public IList<RecentlyAddedProspectModel> GetAllRecentlyAdded(IWebDriver webDriver)
        {
            IList<IWebElement> recentlyAdded = _connectionView.GetRecentlyAdded(webDriver);
            if (recentlyAdded == null)
            {
                return null;
            }

            IList<RecentlyAddedProspectModel> prospects = new List<RecentlyAddedProspectModel>();
            foreach (IWebElement recentlyAddedProspect in recentlyAdded)
            {
                RecentlyAddedProspectModel potentialProspect = new()
                {
                    Name = _connectionView.GetNameFromLiTag(recentlyAddedProspect),
                    ProfileUrl = _connectionView.GetProfileUrlFromLiTag(recentlyAddedProspect)
                };

                prospects.Add(potentialProspect);
            };

            return prospects;
        }

        public int? GetConnectionsCount(IWebDriver webDriver)
        {
            IWebElement connectionHeader = _connectionView.GetConnectionsHeader(webDriver);
            if (connectionHeader == null)
            {
                return null;
            }

            string header = connectionHeader.Text;
            if (string.IsNullOrEmpty(header))
            {
                return null;
            }

            string connectionsCount = header.Split().FirstOrDefault();
            if (string.IsNullOrEmpty(connectionsCount))
            {
                return null;
            }

            if (int.TryParse(connectionsCount, out int result) == false)
            {
                _logger.LogError("Failed to parse connection count from the connection header element. Connection header text was {header}", header);
                return null;
            }

            return result;

        }
    }
}
