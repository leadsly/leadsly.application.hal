using Leadsly.Application.Model;
using Leadsly.Application.Model.Requests.FromHal;
using Leadsly.Application.Model.Responses;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.POMs.Pages
{
    public interface ILinkedInNotificationsPage
    {
        public HalOperationResult<T> ClickNewNotificationsButton<T>(IWebDriver webDriver)
            where T : IOperationResponse;

        public IList<NewProspectConnectionRequest> GatherAllNewProspectInfo(IWebDriver webDriver, string timeZoneId);
        
    }
}
