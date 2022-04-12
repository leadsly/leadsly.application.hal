using Leadsly.Application.Model;
using Leadsly.Application.Model.Responses;
using Leadsly.Application.Model.WebDriver;
using Leadsly.Application.Model.WebDriver.Interfaces;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.POMs.Pages
{
    public abstract class LeadslyWebDriverBase
    {
        public LeadslyWebDriverBase(ILogger logger)
        {
            _logger = logger;
        }

        private readonly ILogger _logger;
        public HalOperationResult<T> GoToPage<T>(IWebDriver webDriver, string pageUrl)
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
    }
}
