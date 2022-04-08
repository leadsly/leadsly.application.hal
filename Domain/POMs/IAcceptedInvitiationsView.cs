using Leadsly.Application.Model.Requests.FromHal;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.POMs
{
    public interface IAcceptedInvitiationsView
    {
        IList<string> GetAllProspectsNames(IWebDriver webDriver);

        IList<NewProspectConnectionRequest> GetAllProspectsInfo(IWebDriver webDriver);
    }
}
