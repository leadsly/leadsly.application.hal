using Domain.Services.Interfaces;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.Extensions;
using System;

namespace Domain.POMs
{
    public static class WebDriverExtensions
    {
        public static void ScrollTop(this IWebDriver webDriver, IHumanBehaviorService humanBehaviorService)
        {
            IWebElement html = webDriver.FindElement(By.XPath("//body"));
            IJavaScriptExecutor js = (IJavaScriptExecutor)webDriver;
            long pageHeight = (long)js.ExecuteScript("return document.body.scrollHeight");
            int totalScrolled = 0;
            while (totalScrolled < pageHeight)
            {
                html.SendKeys(Keys.PageUp);
                totalScrolled += 400;
                humanBehaviorService.RandomWaitMilliSeconds(400, 500);
            }
        }

        public static void ScrollBottom(this IWebDriver webDriver, IHumanBehaviorService humanBehaviorService)
        {
            IWebElement html = webDriver.FindElement(By.XPath("//body"));
            IJavaScriptExecutor js = (IJavaScriptExecutor)webDriver;
            long pageHeight = (long)js.ExecuteScript("return document.body.scrollHeight");
            long totalScrolled = pageHeight;
            while (totalScrolled >= 0)
            {
                html.SendKeys(Keys.PageUp);
                totalScrolled -= 400;
                humanBehaviorService.RandomWaitMilliSeconds(400, 500);
            }
        }

        public static bool IsElementVisible(this IWebDriver webDriver, IWebElement element)
        {
            bool visible = false;
            if (element == null)
            {
                return visible;
            }

            return webDriver.ExecuteJavaScript<bool>(
                                            "var elem = arguments[0],                 " +
                                            "  box = elem.getBoundingClientRect(),    " +
                                            "  cx = box.left + box.width / 2,         " +
                                            "  cy = box.top + box.height / 2,         " +
                                            "  e = document.elementFromPoint(cx, cy); " +
                                            "for (; e; e = e.parentElement) {         " +
                                            "  if (e === elem)                        " +
                                            "    return true;                         " +
                                            "}                                        " +
                                            "return false;                            "
                                            , element);
        }

        public static void ScrollIntoView(this IWebDriver webDriver, IWebElement webElement)
        {
            try
            {
                if (webElement != null)
                {
                    IJavaScriptExecutor js = (IJavaScriptExecutor)webDriver;
                    // accounts for navbar
                    js.ExecuteScript("window.scroll({ top: arguments[0], left: arguments[1], behavior: 'smooth' });", webElement.Location.X, webElement.Location.Y - 140);
                }
            }
            catch (Exception ex)
            {

            }
        }
    }
}
