using Domain.Models;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Responses;
using Leadsly.Application.Model.WebDriver;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Providers.Interfaces
{
    public interface IWebDriverProvider
    {
        HalOperationResult<T> CloseTab<T>(BrowserPurpose browserPurpose, string windowHandleId)
            where T : IOperationResponse;

        HalOperationResult<T> CloseBrowser<T>(BrowserPurpose browserPurpose)
            where T : IOperationResponse;

        HalOperationResult<T> CreateWebDriver<T>(WebDriverOperationData operationData)
            where T : IOperationResponse;

        HalOperationResult<T> GetWebDriver<T>(WebDriverOperationData operationData)
            where T : IOperationResponse;

        HalOperationResult<T> CreateOrGetWebDriver<T>(WebDriverOperationData operationData) 
            where T : IOperationResponse;

        HalOperationResult<T> SwitchTo<T>(IWebDriver webDriver, string windowHandleId)
            where T : IOperationResponse;
    }
}
