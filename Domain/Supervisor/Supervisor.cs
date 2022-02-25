using Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Domain.Supervisor
{
    public partial class Supervisor : ISupervisor
    {
        public Supervisor(ILeadslyBot seleniumStartup, IMemoryCache memoryCache, ILogger<Supervisor> logger)
        {
            _logger = logger;
            _memoryCache = memoryCache;
            _leadslyBot = seleniumStartup;
        }

        private readonly ILogger<Supervisor> _logger;
        private readonly IMemoryCache _memoryCache;
        private readonly ILeadslyBot _leadslyBot;

    }
}
