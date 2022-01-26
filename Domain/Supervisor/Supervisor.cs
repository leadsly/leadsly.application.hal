using Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Domain.Supervisor
{
    public partial class Supervisor : ISupervisor
    {
        public Supervisor(BotLeadslyUserManager userManager, ISeleniumStartup seleniumStartup, ILogger<Supervisor> logger)
        {
            _userManager = userManager;
            _logger = logger;
            _seleniumStartup = seleniumStartup;
        }

        private readonly BotLeadslyUserManager _userManager;
        private readonly ILogger<Supervisor> _logger;
        private readonly ISeleniumStartup _seleniumStartup;

    }
}
