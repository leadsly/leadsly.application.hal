using Leadsly.Application.Model;
using Leadsly.Application.Model.Responses;
using Leadsly.Application.Model.Responses.Hal.Interfaces;
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
        HalOperationResult<T> ClickNotificationsTab<T>(IWebDriver webdriver) where T : IOperationResponse;
        bool AreNewNotifications(IWebDriver webdriver);

        HalOperationResult<T> GetNewConnectionCount<T>(IWebDriver webdriver) where T : IOperationResponse;
    }
}
