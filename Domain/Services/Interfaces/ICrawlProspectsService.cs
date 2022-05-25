using Leadsly.Application.Model.Requests.FromHal;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Services.Interfaces
{
    public interface ICrawlProspectsService
    {
        // bool CrawlProspects(IWebDriver webDriver, string primaryProspectListId, out IList<PrimaryProspectRequest> collectedProspects);
        bool CrawlProspects(IWebDriver webDriver, string primaryProspectListId, out IList<IWebElement> rawCollectedProspects);
        IList<PrimaryProspectRequest> CreatePrimaryProspects(IList<IWebElement> prospects, string primaryProspectListId);
    }
}
