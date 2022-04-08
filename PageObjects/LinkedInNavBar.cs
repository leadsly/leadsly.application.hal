using Domain.POMs;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Responses;
using Leadsly.Application.Model.Responses.Hal.Interfaces;
using Leadsly.Application.Model.Responses.Hal.POMs;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PageObjects
{
    public class LinkedInNavBar : ILinkedInNavBar
    {
        public LinkedInNavBar(ILogger<LinkedInNavBar> logger)
        {
            _logger = logger;
            _rnd = new Random();
        }
        private readonly Random _rnd;
        private readonly ILogger<LinkedInNavBar> _logger;

        private void RandomWait(int minWaitTime, int maxWaitTime)
        {
            int number = _rnd.Next(minWaitTime, maxWaitTime);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            while (sw.Elapsed.TotalSeconds < number)
            {
                continue;
            }
            sw.Stop();
        }

        public HalOperationResult<T> ClickNotificationsTab<T>(IWebDriver webDriver) where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            RandomWait(2, 4);

            IWebElement myNetworkAnchor = NotificationsTab(webDriver);
            if(myNetworkAnchor == null)
            {
                return result;
            }

            try
            {
                myNetworkAnchor.Click();
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Failed to click notifications nav bar control");
                result.Failures.Add(new()
                {
                    Code = Codes.WEBDRIVER_ERROR,
                    Reason = "Could not click on notifications tab in the nav bar",
                    Detail = ex.Message
                });

                return result;
            }

            result.Succeeded = true;
            return result;
        }

        public HalOperationResult<T> IsNewNotification<T>(IWebDriver webdriver) where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            bool? newNotification = null;
            try
            {
                newNotification = NewNotificationBanner != null;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Failed to determine if user has a new connection");
                result.Failures.Add(new()
                {
                    Code = Codes.WEBDRIVER_ERROR,
                    Reason = "Failed to determine if user has gotten new connections",
                    Detail = ex.Message
                });
                return result;
            }

            INotificationNavBarControl myNetwork = new NotificationsNavBarControl
            {
                NewNotification = newNotification
            };

            result.Value = (T)myNetwork;
            result.Succeeded = true;
            return result;
        }

        private IWebElement NewNotificationBanner(IWebDriver webdriver)
        {
            IWebElement newNotificationBanner = default;
            try
            {
                newNotificationBanner = NotificationsTab(webdriver).FindElement(By.ClassName("notification-badge--show")) ?? webdriver.FindElement(By.ClassName("notification-badge--show"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to locate NewConnectionBanner");
            }
            return newNotificationBanner;
        }

        private IWebElement _notificationsAnchor;
        private IWebElement NotificationsTab(IWebDriver webdriver)
        {
            if(_notificationsAnchor == null)
            {
                try
                {
                    _notificationsAnchor = webdriver.FindElement(By.XPath("//div[contains(@class, 'global-nav__primary-link-notif')]/following-sibling::span[contains(text(), 'Notifications')]/parent::a"));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to locate MyNetworkAnchor");
                }                
            }

            return _notificationsAnchor;
        }

        private IWebElement NewConnectionsCount(IWebDriver webDriver)
        {
            IWebElement newConnectionsCount = default;
            try
            {
                newConnectionsCount = NewNotificationBanner(webDriver).FindElement(By.CssSelector("#ember2802 .notification-badge__count")) ?? webDriver.FindElement(By.CssSelector("#ember2802 .notification-badge__count"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to locate new connections banner");
            }

            return newConnectionsCount;
        }

        public HalOperationResult<T> GetNewConnectionCount<T>(IWebDriver webDriver) where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            IWebElement connCountElement = NewConnectionsCount(webDriver);
            if(connCountElement == null)
            {
                result.Failures.Add(new()
                {
                    Code = Codes.WEBDRIVER_ERROR,
                    Reason = "Failed to locate new connections banner",
                    Detail = "Could not locate new connections banner with the count"
                });
                return result;
            }

            int connCount = 0;
            try
            {
                connCount = int.Parse(connCountElement.Text);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error occured parsing connCountElement text property into a number");
                result.Failures.Add(new()
                {
                    Code = Codes.WEBDRIVER_ERROR,
                    Reason = "Failed to extract count number from banner",
                    Detail = "Could not get text property on the connCountElement"
                });
                return result;
            }

            INotificationNavBarControl newNetworkNavBar = new NotificationsNavBarControl
            {
               NotificationCount  = connCount
            };

            result.Value = (T)newNetworkNavBar;
            result.Succeeded = true;
            return result;
        }
    }
}
