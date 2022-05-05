using Domain.Services.Interfaces;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public void RandomClickElement(IWebElement webElement)
        {
            _logger.LogInformation("Clicking the passed in element");

            int number = _rnd.Next(1, 5);

            try
            {
                if(webElement != null)
                {
                    if (number == 3 || number == 5)
                    {
                        webElement.Click();
                        _logger.LogInformation("Executing random click. Number is equal to 3. This means we're clicking the passed in element");
                    }
                }                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to click the passed in element");
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
            catch(Exception ex)
            {
                _logger.LogWarning(ex, "Failed to successfully enter in provided value into the field. Value: {value}", value);
            }
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
