using Leadsly.Application.Model.LinkedInPages.Interface;
using Leadsly.Application.Model.Responses.Hal;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.LinkedInPages
{
    public class ScrapedHtmlElements : IScrapedHtmlElements
    {
        public IWebElement HtmlElement { get; set; }
        public IReadOnlyCollection<IWebElement> HtmlElements { get; set; }
        public OperationInformation OperationInformation { get; set; } = new();
    }
}
