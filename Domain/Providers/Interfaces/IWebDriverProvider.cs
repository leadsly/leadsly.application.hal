using Domain.Models.Requests;
using Domain.MQ.Messages;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Responses;
using OpenQA.Selenium;

namespace Domain.Providers.Interfaces
{
    public interface IWebDriverProvider
    {
        HalOperationResult<T> CloseBrowser<T>(BrowserPurpose browserPurpose)
            where T : IOperationResponse;

        HalOperationResult<T> CreateWebDriver<T>(WebDriverOperationData operationData, string namespaceName, string serviceDiscoveryName)
            where T : IOperationResponse;

        IWebDriver GetOrCreateWebDriver(BrowserPurpose browserPurpose, PublishMessageBody mqMessage);

        IWebDriver GetOrCreateWebDriver(BrowserPurpose browserPurpose, LinkedInSignInRequest request, out bool isNewWebdriver);

        IWebDriver GetWebDriver(BrowserPurpose browserPurpose);

        HalOperationResult<T> SwitchTo<T>(IWebDriver webDriver, string windowHandleId)
            where T : IOperationResponse;

        HalOperationResult<T> SwitchToOrNewTab<T>(IWebDriver webDriver, string windowHandleId)
            where T : IOperationResponse;

        HalOperationResult<T> NewTab<T>(IWebDriver webDriver)
            where T : IOperationResponse;

        bool Refresh(IWebDriver webDriver);
    }
}
