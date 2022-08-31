using OpenQA.Selenium;

namespace Domain.Services.Interfaces
{
    public interface IHumanBehaviorService
    {
        void RandomWaitMilliSeconds(int minWaitTimeMiliseconds, int maxWaitTimeMiliseconds);

        void RandomWaitSeconds(int minWaitTime, int maxWaitTime);

        void RandomWaitMinutes(int minWaitTime, int maxWaitTime);

        void RandomClickElement(IWebElement webElement);

        void EnterValues(IWebElement element, string value, int minMiliseconds, int maxMiliseconds);
        void EnterValue(IWebElement element, char value, int minMiliseconds, int maxMiliseconds);
    }
}
