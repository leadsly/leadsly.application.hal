using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using System;

namespace Domain.Orchestrators
{
    public abstract class PhaseOrchestratorBase
    {
        public PhaseOrchestratorBase(ILogger logger)
        {
            _logger = logger;
        }

        private ILogger _logger;
        protected string PrimaryWindowHandle { get; set; }

        protected virtual bool GoToPage(IWebDriver webDriver, string pageUrl)
        {
            bool succeeded = false;
            if (webDriver.Url.Contains(pageUrl) == false)
            {
                try
                {
                    _logger.LogTrace("Starting navigation to {pageUrl}", pageUrl);
                    webDriver.Navigate().GoToUrl(new Uri(pageUrl));
                    succeeded = true;
                    _logger.LogTrace("Successfully navigated to {pageUrl}", pageUrl);
                }
                catch (WebDriverTimeoutException timeoutEx)
                {
                    _logger.LogError(timeoutEx, "WebDriver WebDriverTimeoutException during navigation to url {pageUrl}", pageUrl);
                    succeeded = false;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to navigate to page {pageUrl}");
                    succeeded = false;
                }
            }
            else
            {
                succeeded = true;
            }

            return succeeded;
        }

        protected bool PrepareBrowserWindow(IWebDriver webDriver, string pageUrl)
        {
            if (SwitchToNewTab(webDriver) == false)
            {
                return false;
            }

            if (GoToPage(webDriver, pageUrl) == false)
            {
                return false;
            }

            return true;
        }

        private bool SwitchToNewTab(IWebDriver webDriver)
        {
            bool succeeded = false;
            try
            {
                webDriver.SwitchTo().NewWindow(WindowType.Tab);
                succeeded = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to switch to new tab");
            }

            return succeeded;
        }

        protected virtual bool SwitchBackToMainTab(IWebDriver webDriver)
        {
            _logger.LogDebug("Navigating back to the primary tab");
            bool succeeded = false;
            try
            {
                webDriver.Close();
                webDriver.SwitchTo().Window(PrimaryWindowHandle);
                succeeded = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to switch webdriver back to the original tab");
            }

            return succeeded;
        }
    }
}
