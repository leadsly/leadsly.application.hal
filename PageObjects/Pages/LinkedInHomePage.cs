using Domain.POMs.Pages;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Responses;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;

namespace PageObjects.Pages
{
    public class LinkedInHomePage : LeadslyBase, ILinkedInHomePage
    {
        public LinkedInHomePage(ILogger<LinkedInHomePage> logger) : base(logger)
        {            
            this._logger = logger;
            
        }
        private readonly ILogger<LinkedInHomePage> _logger;

        public HalOperationResult<T> GoToPage<T>(IWebDriver webDriver, string pageUrl)
            where T : IOperationResponse
        {
            return base.GoToPageUrl<T>(webDriver, pageUrl);
        }
    }
}
