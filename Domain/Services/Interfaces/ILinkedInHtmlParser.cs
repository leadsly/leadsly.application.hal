using Leadsly.Application.Model;
using Leadsly.Application.Model.Responses;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Services.Interfaces
{
    public interface ILinkedInHtmlParser
    {
        HalOperationResult<T> ParseMyNetworkConnections<T>(IReadOnlyCollection<IWebElement> myNetworkNewConnections) where T : IOperationResponse;
    }
}
