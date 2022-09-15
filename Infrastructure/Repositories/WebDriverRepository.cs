using Domain.Models;
using Domain.OptionsJsonModels;
using Domain.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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
                ProfilesVolume = _webDriverOptions.ProfilesVolume,
                UseGrid = _webDriverOptions.UseGrid,
                PagLoadTimeout = _webDriverOptions.PageLoadTimeout,
                SeleniumGrid = new()
                {
                    Url = _webDriverOptions.SeleniumGridConfigOptions.Url,
                    Port = _webDriverOptions.SeleniumGridConfigOptions.Port
                },
                ChromeProfileConfigOptions = new()
                {
                    Proxy = new()
                    {
                        HttpProxy = _webDriverOptions.ChromeConfigOptions.Proxy?.HttpProxy
                    },
                    DefaultChromeProfileName = _webDriverOptions.ChromeConfigOptions.DefaultProfile,
                    DefaultChromeUserProfilesDir = _webDriverOptions.ChromeConfigOptions.ChromeUserDirectory,
                    AddArguments = _webDriverOptions.ChromeConfigOptions.AddArguments
                },
                DefaultImplicitWait = _webDriverOptions.DefaultImplicitWait
            };
        }
    }
}
