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
        public HumanBehaviorService(ILogger<HumanBehaviorService> logger)
        {
            _logger = logger;
            _rnd = new Random();
        }

        private readonly Random _rnd;
        private readonly ILogger<HumanBehaviorService> _logger;

        public void RandomClickElement(IWebElement webElement)
        {
            _logger.LogInformation("Clicking the passed in element");

            int number = _rnd.Next(1, 5);

            try
            {
                if (number == 3)
                {
                    webElement.Click();
                    _logger.LogInformation("Executing random click. Number is equal to 3. This means we're clicking the passed in element");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to click the passed in element");
            }
        }

        public void RandomWait(int minWaitTime, int maxWaitTime)
        {
            int number = _rnd.Next(minWaitTime, maxWaitTime);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            _logger.LogInformation("Entering random wait time. Waiting for {number}", number);
            while (sw.Elapsed.TotalSeconds < number)
            {
                continue;
            }
            sw.Stop();
            _logger.LogInformation("Finished waiting moving on.");
        }
    }
}
