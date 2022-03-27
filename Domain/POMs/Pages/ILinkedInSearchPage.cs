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
    public interface ILinkedInSearchPage
    {
        HalOperationResult<T> GetTotalSearchResults<T>(IWebDriver driver)
            where T : IOperationResponse;

        HalOperationResult<T> GatherProspects<T>(IWebDriver driver)
            where T : IOperationResponse;
    }
}
