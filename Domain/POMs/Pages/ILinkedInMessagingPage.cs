using Leadsly.Application.Model;
using Leadsly.Application.Model.Responses;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.POMs.Pages
{
    public interface ILinkedInMessagingPage
    {
        HalOperationResult<T> ClickCreateNewMessage<T>(IWebDriver webDriver)
            where T : IOperationResponse;

        HalOperationResult<T> EnterProspectsName<T>(IWebDriver webDriver, string name)
            where T : IOperationResponse;

        HalOperationResult<T> EnterMessageContent<T>(IWebDriver webDriver, string messageContent)
            where T : IOperationResponse;

        HalOperationResult<T> ClickSend<T>(IWebDriver webDriver)
            where T : IOperationResponse;
    }
}
