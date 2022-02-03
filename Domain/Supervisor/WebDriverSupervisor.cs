using Domain.Models;
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
        public WebDriverInformation CreateWebDriver(CreateWebDriver newWebDriver)
        {
            IWebDriver driver = newWebDriver.Options != null ? new ChromeDriver(newWebDriver.Options) : new ChromeDriver();
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(newWebDriver.DefaultTimeoutInSeconds);
            WebDriverInformation webDriverInfo = new WebDriverInformation
            {
                Id = newWebDriver.WebDriverId,
                UserId = newWebDriver.UserId,
                WebDriver = driver
            };

            _webDriverManager.Set(webDriverInfo);

            return webDriverInfo;
        }

        public bool DestroyWebDriver(DestroyWebDriver destroyWebDriver)
        {
            WebDriverInformation webDriverInfo = _webDriverManager.Get(destroyWebDriver.WebDriverId);
            _webDriverManager.Remove(webDriverInfo);
            IWebDriver webDriver = webDriverInfo.WebDriver;
            webDriver.Dispose();            
            return true;
        }
    }
}
