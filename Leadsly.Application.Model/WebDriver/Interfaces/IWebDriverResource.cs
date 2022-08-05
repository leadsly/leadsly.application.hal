using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.WebDriver.Interfaces
{
    public interface IWebDriverResource
    {
        public IWebDriver WebDriver { get; }
    }
}
