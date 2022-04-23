using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Services.Interfaces
{
    public interface IHumanBehaviorService
    {
        void RandomWait(int minWaitTime, int maxWaitTime);

        void RandomClickElement(IWebElement webElement);
    }
}
