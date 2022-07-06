using Leadsly.Application.Model;
using Leadsly.Application.Model.Responses;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using System;
using System.Diagnostics;

namespace PageObjects.Pages
{
    public abstract class LeadslyBase
    {
        public LeadslyBase(ILogger logger)
        {
            _logger = logger;
            _rnd = new Random();
        }

        private readonly ILogger _logger;
        private readonly Random _rnd;

        protected HalOperationResult<T> GoToPageUrl<T>(IWebDriver webDriver, string pageUrl)
            where T : IOperationResponse
        {
            _logger.LogInformation("Navigating to url {pageUrl}", pageUrl);

            HalOperationResult<T> result = new();

            try
            {
                _logger.LogTrace("Starting navigation to {pageUrl}", pageUrl);
                webDriver.Navigate().GoToUrl(new Uri(pageUrl));
                _logger.LogTrace("Successfully navigated to {pageUrl}", pageUrl);
            }
            catch (WebDriverTimeoutException timeoutEx)
            {
                _logger.LogError(timeoutEx, "WebDriver WebDriverTimeoutException during navigation to url {pageUrl}", pageUrl);
                throw timeoutEx;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to navigate to page {pageUrl}");
                return result;
            }

            result.Succeeded = true;
            return result;
        }

        protected void NavigateToPage(IWebDriver webDriver, string pageUrl)
        {
            try
            {
                webDriver.Navigate().GoToUrl(new Uri(pageUrl));
            }
            catch (WebDriverTimeoutException timeoutEx)
            {
                throw timeoutEx;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to navigate to page {pageUrl}");
                throw ex;
            }
        }

        protected void RandomWait(int minWaitTime, int maxWaitTime)
        {
            int number = _rnd.Next(minWaitTime, maxWaitTime);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            _logger.LogInformation("Entering random wait time. Waiting for {number}", number);
            while (sw.Elapsed.TotalSeconds < number)
            {
                continue;
            }
            sw.Stop();
            _logger.LogInformation("Finished waiting moving on.");
        }

        protected void RandomClickElement(IWebElement webElement)
        {
            _logger.LogInformation("Clicking the passed in element");
            try
            {
                webElement.Click();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to click the passed in element");
            }
        }
    }
}
