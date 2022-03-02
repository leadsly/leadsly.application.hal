using Domain.Models;
using Leadsly.Application.Model;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Supervisor
{
    public partial class Supervisor : ISupervisor
    {
        public IWebDriverInformation CreateWebDriver(InstantiateWebDriver newWebDriver)
        {
            WebDriverInformation result = new()
            {
                Succeeded = false
            };

            IWebDriver driver = null;
            try
            {
                // [OMikolajczyk_3-1-2022] Keeping as proof of concept for how to request new web drivers.
                // string chromeProfileName = $"Chrome_Profile_{Guid.NewGuid()}";
                // copy template profile into default chrome directory and re-name it
                WebDriverOptions webDriverOptions = _webDriverRepository.GetWebDriverOptions();
                // IOperationResult copyResult = _fileManager.CloneDefaultChromeProfile(chromeProfileName, webDriverOptions);
                ChromeOptions options = SetChromeOptions(webDriverOptions.DefaultChromeProfileName, webDriverOptions.DefaultChromeUserProfilesDir);
                driver = new ChromeDriver(options);
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(newWebDriver.DefaultTimeoutInSeconds);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Failed to create new web driver.");
                result.Succeeded = false;
                result.Failures.Add(new()
                {
                    Code = Codes.WEBDRIVER_ERROR,
                    Detail = ex.Message,
                    Reason = "Failed to create web driver instance"
                });
                return result;
            }


            result.WebDriverId = Guid.NewGuid().ToString();
            _memoryCache.Set(result.WebDriverId, driver, TimeSpan.FromDays(2));
            result.Succeeded = true;
            return result;
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
