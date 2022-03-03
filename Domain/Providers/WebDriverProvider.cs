using Domain.Models;
using Domain.Repositories;
using Domain.Services;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Responses;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Providers
{
    public class WebDriverProvider : IWebDriverProvider
    {
        public WebDriverProvider(IWebDriverRepository webDriverRepository, IWebDriverService webDriverService, ILogger<WebDriverProvider> logger)
        {
            _logger = logger;
            _webDriverRepository = webDriverRepository;
            _webDriverService = webDriverService;
        }

        private readonly ILogger<WebDriverProvider> _logger;
        private readonly IWebDriverRepository _webDriverRepository;
        private readonly IWebDriverService _webDriverService;
        public HalOperationResult<T> CloseTab<T>(string windowHandleId) where T : IOperationResponse
        {
            return _webDriverService.CloseTab<T>(windowHandleId);
        }

        public IWebDriverInformation CreateWebDriver(InstantiateWebDriver newWebDriver)
        {
            WebDriverOptions webDriverOptions = _webDriverRepository.GetWebDriverOptions();
            ChromeOptions options = SetChromeOptions(webDriverOptions.DefaultChromeProfileName, webDriverOptions.DefaultChromeUserProfilesDir);
            return _webDriverService.Create(options, newWebDriver.DefaultTimeoutInSeconds);
        }

        public HalOperationResult<T> SwitchTo<T>(string requestedWindowHandle, out string currentWindowHandle) where T : IOperationResponse
        {
            return _webDriverService.SwitchTo<T>(requestedWindowHandle, out currentWindowHandle);
        }

        private ChromeOptions SetChromeOptions(string profileName, string userDataDir)
        {
            ChromeOptions options = new();
            options.AddArgument("--disable-blink-features=AutomationControlled");
            options.AddArgument("window-size=1280,800");
            options.AddArgument("user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36");
            options.AddArgument(@$"user-data-dir={userDataDir}\{profileName}");

            return options;
        }
    }
}
