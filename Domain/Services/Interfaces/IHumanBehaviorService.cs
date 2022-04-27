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
        void RandomWaitMilliSeconds(int minWaitTimeMiliseconds, int maxWaitTimeMiliseconds);

        void RandomWaitSeconds(int minWaitTime, int maxWaitTime);

        void RandomWaitMinutes(int minWaitTime, int maxWaitTime);

        void RandomClickElement(IWebElement webElement);

        void EnterValues(IWebElement element, string value, int minMiliseconds, int maxMiliseconds);
    }
}
