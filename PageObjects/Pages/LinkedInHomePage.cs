using Domain.Pages;
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
    public class LinkedInHomePage : ILinkedInHomePage
    {
        public LinkedInHomePage(ILogger<LinkedInHomePage> logger)
        {            
            this._logger = logger;
            // this._wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));                   
        }
        private readonly ILogger<LinkedInHomePage> _logger;
        // private readonly WebDriverWait _wait;        



    }
}
