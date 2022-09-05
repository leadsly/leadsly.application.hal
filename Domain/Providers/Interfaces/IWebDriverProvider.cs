﻿using Leadsly.Application.Model;
using Leadsly.Application.Model.Responses;
using Leadsly.Application.Model.WebDriver;
using OpenQA.Selenium;

namespace Domain.Providers.Interfaces
{
    public interface IWebDriverProvider
    {
        HalOperationResult<T> CloseTab<T>(BrowserPurpose browserPurpose, string windowHandleId)
            where T : IOperationResponse;

        HalOperationResult<T> CloseBrowser<T>(BrowserPurpose browserPurpose)
            where T : IOperationResponse;

        HalOperationResult<T> CreateWebDriver<T>(WebDriverOperationData operationData, string namespaceName, string serviceDiscoveryName)
            where T : IOperationResponse;

        IWebDriver GetOrCreateWebDriver(BrowserPurpose browserPurpose, string chromeProfileName, string namespaceName, string serviceDiscoveryName, out bool isNewWebdriver);

        HalOperationResult<T> GetWebDriver<T>(WebDriverOperationData operationData)
            where T : IOperationResponse;

        IWebDriver GetWebDriver(BrowserPurpose browserPurpose);

        HalOperationResult<T> GetOrCreateWebDriver<T>(WebDriverOperationData operationData, string namespaceName, string serviceDiscoveryName)
            where T : IOperationResponse;

        HalOperationResult<T> SwitchTo<T>(IWebDriver webDriver, string windowHandleId)
            where T : IOperationResponse;

        HalOperationResult<T> SwitchToOrNewTab<T>(IWebDriver webDriver, string windowHandleId)
            where T : IOperationResponse;

        HalOperationResult<T> NewTab<T>(IWebDriver webDriver)
            where T : IOperationResponse;

        HalOperationResult<T> Refresh<T>(IWebDriver webDriver)
            where T : IOperationResponse;

        bool Refresh(IWebDriver webDriver);

        bool WebDriverExists(BrowserPurpose browserPurpose);
    }
}
