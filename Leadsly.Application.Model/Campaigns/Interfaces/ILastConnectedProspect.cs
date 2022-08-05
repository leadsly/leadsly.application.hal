using Leadsly.Application.Model.Responses;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.Campaigns.Interfaces
{
    public interface ILastConnectedProspect : IOperationResponse
    {
        public IWebElement LastRequestedProspect { get; set; }
    }
}
