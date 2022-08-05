using Leadsly.Application.Model.Responses;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.LinkedInPages.Interface
{
    public interface IScrapedHtmlElements : IOperationResponse
    {
        public IWebElement HtmlElement { get; set; }
        public IReadOnlyCollection<IWebElement> HtmlElements { get; set; }
    }
}
