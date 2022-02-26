using Domain.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Supervisor
{
    public partial class Supervisor : ISupervisor
    {
        public WebDriverInformation CreateWebDriver(InstantiateWebDriver newWebDriver)
        {
            WebDriverInformation webDriverInfo = new()
            {
                Succeeded = false
            };

            IWebDriver driver = null;
            try
            {
                ChromeOptions options = SetChromeOptions();
                driver = new ChromeDriver(options);
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(newWebDriver.DefaultTimeoutInSeconds);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Failed to create new web driver.");
                webDriverInfo.Succeeded = false;
                return webDriverInfo;
            }

            webDriverInfo.WebDriverId = Guid.NewGuid().ToString();
            webDriverInfo.Succeeded = true;

            _memoryCache.Set(webDriverInfo.WebDriverId, driver, TimeSpan.FromDays(2));

            return webDriverInfo;
        }

        private ChromeOptions SetChromeOptions()
        {
            ChromeOptions options = new();
            options.AddArgument("--disable-blink-features=AutomationControlled");
            options.AddArgument("window-size=1280,800");
            options.AddArgument("user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36");

            return options;
        }

    }
}
