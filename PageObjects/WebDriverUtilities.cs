using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PageObjects
{
    public class WebDriverUtilities : IWebDriverUtilities
    {
        public WebDriverUtilities(ILogger<WebDriverUtilities> logger)
        {
            _logger = logger;
        }

        private readonly ILogger<WebDriverUtilities> _logger;

        public IWebElement WaitUntilNotNull(Func<IWebDriver, IWebElement> searchFunc, IWebDriver webDriver, int waitTimeInSeconds)
        {
            IWebElement elementToFind = default;
            try
            {
                WebDriverWait wait = new WebDriverWait(webDriver, TimeSpan.FromSeconds(waitTimeInSeconds));
                wait.Until(drv =>
                {
                    elementToFind = searchFunc(drv);
                    return elementToFind != null;
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "WebDrivers wait method timedout. This means that the maximum allowed wait time elapsed and the element was not found. Wait time in seconds {waitTimeInSeconds}", waitTimeInSeconds);
            }
            return elementToFind;
        }

        public IList<IWebElement> WaitUntilNotNull(Func<IWebDriver, IList<IWebElement>> searchFunc, IWebDriver webDriver, int waitTimeInSeconds)
        {
            IList<IWebElement> elementsToFind = default;
            try
            {
                WebDriverWait wait = new WebDriverWait(webDriver, TimeSpan.FromSeconds(waitTimeInSeconds));
                
                wait.Until(drv =>
                {
                    elementsToFind = searchFunc(drv);
                    return elementsToFind != null;
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "WebDrivers wait method timedout. This means that the maximum allowed wait time elapsed and the element was not found. Wait time in seconds {waitTimeInSeconds}", waitTimeInSeconds);
            }
            return elementsToFind;
        }
    }
}
