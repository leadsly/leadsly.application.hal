using Leadsly.Application.Model.LinkedInPages.SearchResultPage.Interfaces;
using Leadsly.Application.Model.Responses.Hal;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.LinkedInPages.SearchResultPage
{
    public class GatherProspects : IGatherProspects
    {
        public List<IWebElement> ProspectElements { get; set; }
        public OperationInformation OperationInformation { get; set; }
    }
}
