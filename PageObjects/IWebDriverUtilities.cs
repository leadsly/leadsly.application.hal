using OpenQA.Selenium;
using System;
using System.Collections.Generic;

namespace PageObjects
{
    public interface IWebDriverUtilities
    {
        IWebElement WaitUntilNotNull(Func<IWebDriver, IWebElement> searchFunc, IWebDriver webDriver, int waitTimeInSeconds);
        IWebElement WaitUntilNotNull(Func<IWebElement, IWebElement> searchFunc, IWebElement startingElement, IWebDriver webDriver, int waitTimeInSeconds);
        IWebElement WaitUntilNull(Func<IWebDriver, IWebElement> searchFunc, IWebDriver webDriver, int waitTimeInSeconds);
        IList<IWebElement> WaitUntilNotNull(Func<IWebDriver, IList<IWebElement>> searchFunc, IWebDriver webDriver, int waitTimeInSeconds);
        bool HandleClickElement(IWebElement element);
        public void ScrollIntoView(IWebElement webElement, IWebDriver webDriver);
    }
}
