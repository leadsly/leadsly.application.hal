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
        public WebDriverRepository(ILogger<WebDriverRepository> logger, IOptions<ChromeProfileOptions> webDriverOptions)
        {
            _logger = logger;
            _webDriverOptions = webDriverOptions.Value;
        }

        private readonly ILogger<WebDriverRepository> _logger;
        private readonly ChromeProfileOptions _webDriverOptions;

        public WebDriverOptions GetWebDriverOptions()
        {
            return new WebDriverOptions
            {
                DefaultChromeProfileName = _webDriverOptions.DefaultProfile,
                DefaultChromeUserProfilesDir = _webDriverOptions.ChromeUserDirectory
            };
        }
    }
}
