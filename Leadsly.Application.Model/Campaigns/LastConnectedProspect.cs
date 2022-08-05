using Leadsly.Application.Model.Campaigns.Interfaces;
using Leadsly.Application.Model.Responses.Hal;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.Campaigns
{
    public class LastConnectedProspect : ILastConnectedProspect
    {
        public OperationInformation OperationInformation { get; set; }
        public IWebElement LastRequestedProspect { get; set; }
    }
}
