using Domain.Facades.Interfaces;
using Domain.Providers.Interfaces;
using Domain.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Domain.Supervisor
{
    public partial class Supervisor : ISupervisor
    {
        public Supervisor(
            IWebDriverProvider webDriverProvider,
            IHumanBehaviorService humanBehaviorService,
            ILinkedInPageFacade linkedInPageFacade,
            ILogger<Supervisor> logger)
        {
            _humanBehaviorService = humanBehaviorService;
            _logger = logger;
            _linkedInPageFacade = linkedInPageFacade;
            _webDriverProvider = webDriverProvider;
        }

        private readonly ILinkedInPageFacade _linkedInPageFacade;
        private readonly IWebDriverProvider _webDriverProvider;
        private readonly IHumanBehaviorService _humanBehaviorService;
        private readonly ILogger<Supervisor> _logger;

    }
}
