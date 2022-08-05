using Leadsly.Application.Model.Responses.Hal;
using Leadsly.Application.Model.WebDriver.Interfaces;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.WebDriver
{
    public class GetOrCreateWebDriverOperation : IGetOrCreateWebDriverOperation
    {
        public IWebDriver WebDriver { get; set; }

        public OperationInformation OperationInformation { get; set; } = new();
    }
}
