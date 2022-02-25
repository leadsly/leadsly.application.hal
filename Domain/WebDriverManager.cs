using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain
{
    public class WebDriverManager : IWebDriverManager
    {
        private static HashSet<WebDriverInformation> _webDrivers = new HashSet<WebDriverInformation>();
        public WebDriverInformation Get(string id)
        {
            return _webDrivers.FirstOrDefault(x => x.WebDriverId == id);
        }

        public void Set(WebDriverInformation webdriverInfo)
        {
            _webDrivers.Add(webdriverInfo);
        }

        public void Remove(WebDriverInformation webdriverInfo)
        {
            _webDrivers.Remove(webdriverInfo);
        }

    }
}
