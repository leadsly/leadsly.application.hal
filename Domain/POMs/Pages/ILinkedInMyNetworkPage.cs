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
    public interface ILinkedInMyNetworkPage
    {
        HalOperationResult<T> CollectAllNewConnections<T>(IWebDriver webdriver) where T : IOperationResponse;
    }
}
