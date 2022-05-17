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
        public WebDriverRepository(ILogger<WebDriverRepository> logger, IOptions<WebDriverConfigOptions> webDriverOptions)
        {
            _logger = logger;
            _webDriverOptions = webDriverOptions.Value;
        }

        private readonly ILogger<WebDriverRepository> _logger;
        private readonly WebDriverConfigOptions _webDriverOptions;

        public WebDriverOptions GetWebDriverOptions()
        {
            return new WebDriverOptions
            {
                SeleniumGrid = new()
                {
                    Url = _webDriverOptions.SeleniumGridConfigOptions.Url,
                    Port = _webDriverOptions.SeleniumGridConfigOptions.Port
                },
                ChromeProfileConfigOptions = new()
                {
                    DefaultChromeProfileName = _webDriverOptions.ChromeConfigOptions.DefaultProfile,
                    DefaultChromeUserProfilesDir = _webDriverOptions.ChromeConfigOptions.ChromeUserDirectory,
                    AddArguments = _webDriverOptions.ChromeConfigOptions.AddArguments
                },
                DefaultImplicitWait = _webDriverOptions.DefaultImplicitWait                
            };
        }
    }
}
