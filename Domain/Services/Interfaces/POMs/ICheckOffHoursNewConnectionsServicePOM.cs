﻿using Domain.Models.MonitorForNewProspects;
using OpenQA.Selenium;
using System.Collections.Generic;

namespace Domain.Services.Interfaces.POMs
{
    public interface ICheckOffHoursNewConnectionsServicePOM
    {
        IList<RecentlyAddedProspectModel> GetAllRecentlyAddedSince(IWebDriver webDriver, int numOfHoursAgo, string timezoneId);
    }
}
