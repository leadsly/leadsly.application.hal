using Domain.Models;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Providers
{
    public interface IWebDriverProvider
    {
        HalOperationResult<T> SwitchTo<T>(string requestedWindowHandle, out string currentWindowHandle)
            where T : IOperationResponse;

        IWebDriverInformation CreateWebDriver(InstantiateWebDriver newWebDriver);

        HalOperationResult<T> CloseTab<T>(string windowHandleId)
            where T : IOperationResponse;
    }
}
