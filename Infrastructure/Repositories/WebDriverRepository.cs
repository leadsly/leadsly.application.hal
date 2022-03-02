using Domain.Models;
using Domain.OptionsJsonModels;
using Domain.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class WebDriverRepository : IWebDriverRepository
    {
        public WebDriverRepository(ILogger<WebDriverRepository> logger, IOptions<ChromeConfigOptions> webDriverOptions)
        {
            _logger = logger;
            _webDriverOptions = webDriverOptions.Value;
        }

        private readonly ILogger<WebDriverRepository> _logger;
        private readonly ChromeConfigOptions _webDriverOptions;

        public WebDriverOptions GetWebDriverOptions()
        {
            return new WebDriverOptions
            {
                DefaultChromeProfileName = _webDriverOptions.DefaultProfile,
                DefaultChromeUserProfilesDir = _webDriverOptions.ChromeUserDirectory,
                AddArguments = _webDriverOptions.AddArguments,
                WebDriverWaitFromSeconds = _webDriverOptions.WebDriverWaitFromSeconds
            };
        }
    }
}
