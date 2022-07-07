using Domain.Facades.Interfaces;
using Domain.Providers.Interfaces;
using Domain.Repositories;
using Domain.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Domain.Supervisor
{
    public partial class Supervisor : ISupervisor
    {
        public Supervisor(
            IWebDriverRepository webDriverRepository,
            IHalAuthProvider halAuthProvider,
            IFileManager fileManager,
            IWebDriverProvider webDriverProvider,
            IMemoryCache memoryCache,
            IHumanBehaviorService humanBehaviorService,
            ILinkedInPageFacade linkedInPageFacade,
            IWebDriverManagerProvider webDriverManagerProvider,
            ILogger<Supervisor> logger)
        {
            _humanBehaviorService = humanBehaviorService;
            _logger = logger;
            _memoryCache = memoryCache;
            _fileManager = fileManager;
            _linkedInPageFacade = linkedInPageFacade;
            _webDriverRepository = webDriverRepository;
            _halAuthProvider = halAuthProvider;
            _webDriverProvider = webDriverProvider;
            _webDriverManagerProvider = webDriverManagerProvider;
        }

        private readonly ILinkedInPageFacade _linkedInPageFacade;
        private readonly IWebDriverManagerProvider _webDriverManagerProvider;
        private readonly IWebDriverProvider _webDriverProvider;
        private readonly IHalAuthProvider _halAuthProvider;
        private readonly IHumanBehaviorService _humanBehaviorService;
        private readonly ILogger<Supervisor> _logger;
        private readonly IFileManager _fileManager;
        private readonly IMemoryCache _memoryCache;
        private readonly IWebDriverRepository _webDriverRepository;

    }
}
