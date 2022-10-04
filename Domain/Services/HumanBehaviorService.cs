using Domain.Services.Interfaces;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using System;
using System.Diagnostics;

namespace Domain.Services
{
    public class HumanBehaviorService : IHumanBehaviorService
    {
        public HumanBehaviorService(ILogger<HumanBehaviorService> logger, Random random)
        {
            _logger = logger;
            _rnd = random;
        }

        private readonly Random _rnd;
        private readonly ILogger<HumanBehaviorService> _logger;
        private const string ErrorString = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        public void RandomClickElement(IWebElement webElement)
        {
            _logger.LogTrace("[RandomClickElement] Random click element is executing");

            int number = _rnd.Next(1, 10);

            try
            {
                if (webElement != null)
                {
                    _logger.LogTrace("[RandomClickElement]: The passed in element is not null. This means the element is found.");
                    if (number == 2 || number == 3 || number == 4 || number == 7 || number == 8)
                    {
                        webElement.Click();
                        _logger.LogInformation($"Executing random click. Number is equal to {number}. This means we're clicking the passed in element");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[RandomClickElement] Failed to click the passed in element");
            }
        }

        public void EnterValues(IWebElement element, string value, int minMiliseconds, int maxMiliseconds)
        {
            Stopwatch sw = new Stopwatch();
            try
            {
                foreach (char character in value)
                {
                    int randomWait = _rnd.Next(minMiliseconds, maxMiliseconds);
                    sw.Start();
                    element.SendKeys(character.ToString());
                    while (sw.Elapsed.TotalMilliseconds < randomWait)
                    {
                        continue;
                    }
                    sw.Restart();
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to successfully enter in provided value into the field. Value: {value}", value);
            }
        }

        public void DeleteValue(IWebElement element, string valueToDelete, int minMiliseconds, int maxMiliseconds)
        {
            Stopwatch sw = new Stopwatch();
            int random = _rnd.Next(1, 10);
            try
            {
                if (random == 1 || random == 2 || random == 7 || random == 8)
                {
                    DeleteValue_CTRL_A_DEL(element);
                }
                else if (random == 5 || random == 9 || random == 10 || random == 3 || random == 4)
                {
                    DeleteValue_Backspace(element, valueToDelete, minMiliseconds, maxMiliseconds);
                }
                else
                {
                    element.Clear();
                }

            }
            catch (Exception ex)
            {
                _logger.LogWarning("Failed to successfully send Backspace key to the input field");
            }
        }

        private void DeleteValue_Backspace(IWebElement element, string valueToDelete, int minMiliseconds, int maxMiliseconds)
        {
            Stopwatch sw = new Stopwatch();

            foreach (char character in valueToDelete)
            {
                int randomWait = _rnd.Next(minMiliseconds, maxMiliseconds);

                element.SendKeys(Keys.Backspace);

                sw.Start();
                while (sw.Elapsed.TotalMilliseconds < randomWait)
                {
                    continue;
                }
                sw.Restart();
            }
        }

        private void DeleteValue_CTRL_A_DEL(IWebElement inputField)
        {
            inputField.SendKeys(Keys.Control + "a" + Keys.Delete);
        }

        public void EnterValue(IWebElement element, char value, int minMiliseconds, int maxMiliseconds)
        {
            Stopwatch sw = new Stopwatch();
            try
            {
                int randomWait = _rnd.Next(minMiliseconds, maxMiliseconds);

                ErrorFactor(element, minMiliseconds, maxMiliseconds);
                sw.Start();
                element.SendKeys(value.ToString());
                while (sw.Elapsed.TotalMilliseconds < randomWait)
                {
                    continue;
                }
                sw.Restart();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to successfully enter in provided value into the field. Value: {value}", value);
            }
        }

        private bool ErrorFactor(IWebElement element, int minMiliseconds, int maxMiliseconds)
        {
            Stopwatch sw = new Stopwatch();
            bool errorFactored = false;
            try
            {
                int randomWait = _rnd.Next(minMiliseconds, maxMiliseconds);
                int random = _rnd.Next(1, 15);
                sw.Start();

                if (random == 5 || random == 13)
                {
                    char error = GetRandomCharacter();
                    element.SendKeys(error.ToString());

                    while (sw.Elapsed.TotalMilliseconds < randomWait)
                    {
                        continue;
                    }

                    element.SendKeys(Keys.Backspace);
                }

                sw.Restart();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to enter in error factor");
            }

            return errorFactored;
        }

        private char GetRandomCharacter()
        {
            int index = _rnd.Next(ErrorString.Length);
            return ErrorString[index];
        }

        private void RandomWaitTime(int number)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            while (sw.Elapsed.TotalMilliseconds < number)
            {
                continue;
            }
            sw.Stop();
            _logger.LogInformation("Finished waiting moving on.");
        }

        public void RandomWaitSeconds(int minWaitTime, int maxWaitTime)
        {
            int minWaitMili = minWaitTime * 1000;
            int maxWaitMili = maxWaitTime * 1000;

            int number = _rnd.Next(minWaitMili, maxWaitMili);

            int numInSeconds = number / 1000;
            _logger.LogInformation("Entering random wait time. Waiting for {numInSeconds} seconds", numInSeconds);

            RandomWaitTime(number);
        }

        public void RandomWaitMilliSeconds(int minWaitTimeMiliseconds, int maxWaitTimeMiliseconds)
        {
            int number = _rnd.Next(minWaitTimeMiliseconds, maxWaitTimeMiliseconds);

            int numInSeconds = number / 1000;
            _logger.LogInformation("Entering random wait time. Waiting for {numInSeconds} seconds", numInSeconds);

            RandomWaitTime(number);
        }

        public void RandomWaitMinutes(int minWaitTime, int maxWaitTime)
        {
            int minWaitMili = (int)TimeSpan.FromMinutes(minWaitTime).TotalMilliseconds;
            int maxWaitMili = (int)TimeSpan.FromMinutes(maxWaitTime).TotalMilliseconds;

            int number = _rnd.Next(minWaitMili, maxWaitMili);

            int numInSeconds = number / 1000;
            _logger.LogInformation("Entering random wait time. Waiting for {numInSeconds} seconds", numInSeconds);

            RandomWaitTime(number);
        }
    }
}
