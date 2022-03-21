using Leadsly.Application.Model;
using Leadsly.Application.Model.Responses;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.POMs
{
    public interface ILinkedInNavBar
    {
        HalOperationResult<T> ClickMyNetworkTab<T>(IWebDriver webdriver) where T : IOperationResponse;
        HalOperationResult<T> IsNewConnectionNotification<T>(IWebDriver webdriver) where T : IOperationResponse;

        HalOperationResult<T> GetNewConnectionCount<T>(IWebDriver webdriver) where T : IOperationResponse;
    }
}
