using Domain.Models;
using Microsoft.Extensions.Caching.Memory;
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
            ChromeOptions options = SetChromeOptions();
            IWebDriver driver = new ChromeDriver(options);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(newWebDriver.DefaultTimeoutInSeconds);
            WebDriverInformation webDriverInfo = new WebDriverInformation
            {
                WebDriverId = Guid.NewGuid().ToString(),
            };

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
