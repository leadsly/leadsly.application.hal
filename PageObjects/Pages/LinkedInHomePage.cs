using Domain.Pages;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Responses;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PageObjects.Pages
{
    public class LinkedInHomePage : LeadslyWebDriverBase, ILinkedInHomePage
    {
        public LinkedInHomePage(ILogger<LinkedInHomePage> logger) : base(logger)
        {            
            this._logger = logger;
            
        }
        private readonly ILogger<LinkedInHomePage> _logger;        

    }
}
