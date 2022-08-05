using Leadsly.Application.Model.Responses;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.LinkedInPages.SearchResultPage.Interfaces
{
    public interface IGatherProspects : IOperationResponse
    {
        public List<IWebElement> ProspectElements { get; set; }
    }
}
