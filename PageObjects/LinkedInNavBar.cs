using Domain.POMs;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Responses;
using Leadsly.Application.Model.Responses.Hal.Interfaces;
using Leadsly.Application.Model.Responses.Hal.POMs;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
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
        }

        private readonly ILogger<LinkedInNavBar> _logger;
        public HalOperationResult<T> ClickMyNetworkTab<T>(IWebDriver webDriver) where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            IWebElement myNetworkAnchor = MyNetworkAnchor(webDriver);
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
                _logger.LogError(ex, "Failed to click my network anchor nav bar control");
                result.Failures.Add(new()
                {
                    Code = Codes.WEBDRIVER_ERROR,
                    Reason = "Could not click on my network tab in the nav bar",
                    Detail = ex.Message
                });

                return result;
            }

            result.Succeeded = true;
            return result;
        }

        public HalOperationResult<T> IsNewConnectionNotification<T>(IWebDriver webdriver) where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            bool? newConnection = null;
            try
            {
                newConnection = NewConnectionNotificationBanner != null;
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

            IMyNetworkNavBarControl myNetwork = new MyNetworkNavBarControl
            {
                NewConnection = newConnection
            };

            result.Value = (T)myNetwork;
            result.Succeeded = true;
            return result;
        }

        private IWebElement NewConnectionNotificationBanner(IWebDriver webdriver)
        {
            IWebElement newConnectionsBanner = default;
            try
            {
                newConnectionsBanner = MyNetworkAnchor(webdriver).FindElement(By.ClassName("notification-badge--show")) ?? webdriver.FindElement(By.ClassName("notification-badge--show"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to locate NewConnectionBanner");
            }
            return newConnectionsBanner;
        }

        private IWebElement _myNetworkAnchor;
        private IWebElement MyNetworkAnchor(IWebDriver webdriver)
        {
            if(_myNetworkAnchor == null)
            {
                try
                {
                    _myNetworkAnchor = webdriver.FindElement(By.Id("ember20"));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to locate MyNetworkAnchor");
                }                
            }

            return _myNetworkAnchor;
        }

        private IWebElement NewConnectionsCount(IWebDriver webDriver)
        {
            IWebElement newConnectionsCount = default;
            try
            {
                newConnectionsCount = NewConnectionNotificationBanner(webDriver).FindElement(By.CssSelector("#ember2802 .notification-badge__count")) ?? webDriver.FindElement(By.CssSelector("#ember2802 .notification-badge__count"));
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

            IMyNetworkNavBarControl newNetworkNavBar = new MyNetworkNavBarControl
            {
                ConnectionsCount = connCount
            };

            result.Value = (T)newNetworkNavBar;
            result.Succeeded = true;
            return result;
        }
    }
}
