using Leadsly.Application.Model;
using Leadsly.Application.Model.Responses;
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
            HalOperationResult<T> result = new();

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
                return result;
            }

            result.Succeeded = true;
            return result;
        }
    }
}
