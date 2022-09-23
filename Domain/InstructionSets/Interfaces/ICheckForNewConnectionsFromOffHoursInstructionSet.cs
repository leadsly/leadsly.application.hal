using Domain.Models.MonitorForNewProspects;
using Domain.MQ.Messages;
using OpenQA.Selenium;
using System.Collections.Generic;

namespace Domain.InstructionSets.Interfaces
{
    public interface ICheckForNewConnectionsFromOffHoursInstructionSet
    {
        public IList<RecentlyAddedProspectModel> RecentlyAddedProspects { get; }
        void BeginCheckingForNewConnectionsFromOffHours(IWebDriver webDriver, CheckOffHoursNewConnectionsBody message);
    }
}
