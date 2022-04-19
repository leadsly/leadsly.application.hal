using Domain.Models;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Responses;
using Leadsly.Application.Model.WebDriver;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Services.Interfaces
{
    public interface IWebDriverService
    {
        HalOperationResult<T> SwitchTo<T>(IWebDriver webDriver, WebDriverOperationData operationData, out string currentWindowHandle)
            where T : IOperationResponse;

        HalOperationResult<T> Create<T>(BrowserPurpose browserPurpose, WebDriverOptions webDriverOptions, string chromeProfileName) where T : IOperationResponse;

        HalOperationResult<T> CloseTab<T>(IWebDriver driver, string windowHandleId)
            where T : IOperationResponse;

        HalOperationResult<T> Close<T>(IWebDriver driver)
            where T : IOperationResponse;
    }
}
