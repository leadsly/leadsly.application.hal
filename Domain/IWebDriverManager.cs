using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Models;

namespace Domain
{
    public interface IWebDriverManager
    {
        WebDriverInformation Get(string id);
        void Set(WebDriverInformation webdriverInfo);

        void Remove(WebDriverInformation webdriverInfo);
    }
}
