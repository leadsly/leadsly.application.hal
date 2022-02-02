using Leadsly.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Domain.Supervisor
{
    public partial class Supervisor : ISupervisor
    {
        public Supervisor(BotLeadslyUserManager userManager, ILeadslyBot seleniumStartup, IWebDriverManager webDriverManager, ILogger<Supervisor> logger)
        {
            _userManager = userManager;
            _logger = logger;
            _leadslyBot = seleniumStartup;
            _webDriverManager = webDriverManager;
        }

        private readonly BotLeadslyUserManager _userManager;
        private readonly IWebDriverManager _webDriverManager;
        private readonly ILogger<Supervisor> _logger;
        private readonly ILeadslyBot _leadslyBot;

    }
}
