using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class WebDriverInformation
    {
        public string Id { get; set; }
        public string UserId { get; set; }

        public IWebDriver WebDriver { get; set; }

    }
}
