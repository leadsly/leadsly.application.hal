using Leadsly.Application.Model.Requests.FromHal;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Services.Interfaces
{
    public interface ICampaignProspectsService
    {
        CampaignProspectRequest CreateCampaignProspects(IWebElement prospect, string campaignId);
    }
}
