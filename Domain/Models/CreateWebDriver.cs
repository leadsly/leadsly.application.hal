using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class CreateWebDriver
    {
        public string UserId { get; set; }
        public string WebDriverId { get; set; }
        public ChromeOptions Options { get; set; }
        public long DefaultTimeoutInSeconds { get; set; }
    }
}
