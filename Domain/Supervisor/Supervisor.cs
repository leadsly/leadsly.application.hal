using Domain.Models;
using Domain.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Domain.Supervisor
{
    public partial class Supervisor : ISupervisor
    {
        public Supervisor(ILeadslyBot seleniumStartup, IWebDriverRepository webDriverRepository, IFileManager fileManager, IMemoryCache memoryCache, ILogger<Supervisor> logger)
        {
            _logger = logger;
            _memoryCache = memoryCache;
            _leadslyBot = seleniumStartup;
            _fileManager = fileManager;
            _webDriverRepository = webDriverRepository;
        }

        private readonly ILogger<Supervisor> _logger;
        private readonly IFileManager _fileManager;
        private readonly IMemoryCache _memoryCache;
        private readonly IWebDriverRepository _webDriverRepository;
        private readonly ILeadslyBot _leadslyBot;

    }
}
