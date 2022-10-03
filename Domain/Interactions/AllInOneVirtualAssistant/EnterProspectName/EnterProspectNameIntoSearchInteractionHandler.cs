using Domain.Interactions.AllInOneVirtualAssistant.EnterProspectName.Interfaces;
using Domain.Services.Interfaces;
using Domain.Services.Interfaces.POMs;
using Microsoft.Extensions.Logging;
using System;

namespace Domain.Interactions.AllInOneVirtualAssistant.EnterProspectName
{
    public class EnterProspectNameIntoSearchInteractionHandler : IEnterProspectNameIntoSearchInteractionHandler
    {
        public EnterProspectNameIntoSearchInteractionHandler(
            ILogger<EnterProspectNameIntoSearchInteractionHandler> logger,
            Random random,
            IHumanBehaviorService humanBehaviorService,
            IFollowUpMessageOnConnectionsServicePOM service)
        {
            _logger = logger;
            _rnd = random;
            _humanBehaviorService = humanBehaviorService;
            _service = service;
        }

        private readonly Random _rnd;
        private readonly IHumanBehaviorService _humanBehaviorService;
        private readonly ILogger<EnterProspectNameIntoSearchInteractionHandler> _logger;
        private readonly IFollowUpMessageOnConnectionsServicePOM _service;
        public bool HandleInteraction(InteractionBase interaction)
        {
            EnterProspectNameIntoSearchInteraction enterProspectInteraction = interaction as EnterProspectNameIntoSearchInteraction;
            int random = _rnd.Next(1, 5);
            string prospectName = enterProspectInteraction.ProspectName;
            if (random == 2 || random == 3 || random == 4)
            {
                prospectName = prospectName.ToLower();
            }

            bool? succeeded = _service.EnterProspectName(enterProspectInteraction.WebDriver, prospectName);
            if (succeeded == null || succeeded == false)
            {
                _humanBehaviorService.RandomWaitMilliSeconds(1500, 2500);
                // handle any failures if desired, try again
                succeeded = _service.EnterProspectName(enterProspectInteraction.WebDriver, prospectName);
                if (succeeded == false || succeeded == null)
                {
                    _logger.LogError("Another attempt to enter prospect name into search field succeeded");
                    return false;
                }
            }

            return true;
        }
    }
}
