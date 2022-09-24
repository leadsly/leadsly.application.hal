using Domain.InstructionSets.Interfaces;
using Domain.Interactions;
using Domain.Interactions.CheckOffHoursNewConnections.GetAllRecentlyAddedSince;
using Domain.Interactions.CheckOffHoursNewConnections.GetAllRecentlyAddedSince.Interfaces;
using Domain.Models.MonitorForNewProspects;
using Domain.MQ.Messages;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using System.Collections.Generic;

namespace Domain.InstructionSets
{
    public class CheckForNewConnectionsFromOffHoursInstructionSet : ICheckForNewConnectionsFromOffHoursInstructionSet
    {
        public CheckForNewConnectionsFromOffHoursInstructionSet(
            ILogger<CheckForNewConnectionsFromOffHoursInstructionSet> logger,
            IGetAllRecentlyAddedSinceInteractionHandler interactionHandler)
        {
            _logger = logger;
            _interactionHandler = interactionHandler;
        }

        private readonly ILogger<CheckForNewConnectionsFromOffHoursInstructionSet> _logger;
        private readonly IGetAllRecentlyAddedSinceInteractionHandler _interactionHandler;

        public IList<RecentlyAddedProspectModel> RecentlyAddedProspects => _interactionHandler.GetRecentlyAddedProspects();

        public void BeginCheckingForNewConnectionsFromOffHours(IWebDriver webDriver, CheckOffHoursNewConnectionsBody message)
        {
            bool succeeded = GetRecentlyAddedInteraction(webDriver, message);
            if (succeeded == false)
            {
                _logger.LogDebug("Failed to get recently added prospects from CheckOffHoursConnectionsPhase");
            }
        }

        private bool GetRecentlyAddedInteraction(IWebDriver webDriver, CheckOffHoursNewConnectionsBody message)
        {
            InteractionBase interaction = new GetAllRecentlyAddedSinceInteraction
            {
                WebDriver = webDriver,
                NumOfHoursAgo = message.NumOfHoursAgo,
                TimezoneId = message.TimeZoneId
            };

            return _interactionHandler.HandleInteraction(interaction);
        }
    }
}
