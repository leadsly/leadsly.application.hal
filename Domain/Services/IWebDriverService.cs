using Domain.Models;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Responses;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Services
{
    public interface IWebDriverService
    {
        HalOperationResult<T> SwitchTo<T>(string requestedWindowHandle, out string currentWindowHandle)
            where T : IOperationResponse;

        IWebDriverInformation Create(ChromeOptions options, long implicitDefaultTimeout);

        HalOperationResult<T> CloseTab<T>(string windowHandleId)
            where T : IOperationResponse;
    }
}
