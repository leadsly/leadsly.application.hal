using Domain.Models;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Responses;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Collections.Generic;

namespace Domain.Services.Interfaces
{
    public interface IWebDriverService
    {
        HalOperationResult<T> SwitchTo<T>(IWebDriver webDriver, WebDriverOperationData operationData, out string currentWindowHandle)
            where T : IOperationResponse;

        HalOperationResult<T> Create<T>(BrowserPurpose browserPurpose, WebDriverOptions webDriverOptions, string chromeProfileName, string namespaceName, string serviceDiscoveryName) where T : IOperationResponse;

        IWebDriver Create(ChromeOptions options, WebDriverOptions webDriverOptions, string gridNamespaceName, string gridCloudMapServiceName);
        ChromeOptions SetChromeOptions(IList<string> addArguments, string chromeProfile, string defaultChromeProfileDir);

        HalOperationResult<T> CloseTab<T>(IWebDriver driver, string windowHandleId)
            where T : IOperationResponse;

        HalOperationResult<T> Close<T>(IWebDriver driver)
            where T : IOperationResponse;
    }
}
