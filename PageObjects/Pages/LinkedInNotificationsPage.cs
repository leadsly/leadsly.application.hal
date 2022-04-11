using Domain.POMs;
using Domain.POMs.Pages;
using Domain.Providers.Interfaces;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns.Interfaces;
using Leadsly.Application.Model.Campaigns.MonitorForNewProspects;
using Leadsly.Application.Model.Requests.FromHal;
using Leadsly.Application.Model.Responses;
using Leadsly.Application.Model.WebDriver;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PageObjects.Pages
{
    public class LinkedInNotificationsPage : ILinkedInNotificationsPage
    {
        public LinkedInNotificationsPage(ILogger<LinkedInNotificationsPage> logger, IAcceptedInvitiationsView acceptedInvitationsView, IWebDriverProvider webDriverProvider)
        {
            _logger = logger;
            _acceptedInvitiationsView = acceptedInvitationsView;
            _webDriverProvider = webDriverProvider;
            _rnd = new Random();
        }

        private ILogger<LinkedInNotificationsPage> _logger;
        private IAcceptedInvitiationsView _acceptedInvitiationsView;
        private readonly IWebDriverProvider _webDriverProvider;
        private readonly Random _rnd;

        private IWebElement NewNotificationsButton(IWebDriver webDriver)
        {
            IWebElement newNotificationsButton = default;
            try
            {
                newNotificationsButton = webDriver.FindElement(By.CssSelector("button[aria-label='Load new notifications']"));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to locate new notifications button");
            }
            return newNotificationsButton;
        }

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

        /// <summary>
        /// Clicks the 'New notifications' button that shows up on the notifications view when a new notification is present
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="webDriver"></param>
        /// <returns></returns>
        public HalOperationResult<T> ClickNewNotificationsButton<T>(IWebDriver webDriver)
            where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            IWebElement newNotificationsButton = NewNotificationsButton(webDriver);
            if(newNotificationsButton == null)
            {
                return result;
            }

            RandomWait(2, 4);

            newNotificationsButton.Click();

            result.Succeeded = true;
            return result;
        }

        private IReadOnlyCollection<IWebElement> GetAllNewNotifications(IWebDriver webDriver)
        {
            IReadOnlyCollection<IWebElement> newNotificationsElements = default;
            try
            {
                newNotificationsElements = webDriver.FindElements(By.XPath("//section[contains(@class, 'artdeco-card')]//article[contains(@class, 'nt-card--unread')]"));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not find any new notifications");
            }
            return newNotificationsElements;
        }

        public IList<NewProspectConnectionRequest> GatherAllNewProspectInfo(IWebDriver webDriver, string timeZoneId)
        {
            List<NewProspectConnectionRequest> newlyAcceptedProspectNames = new List<NewProspectConnectionRequest>();            
            IReadOnlyCollection<IWebElement> newNotifications = GetAllNewNotifications(webDriver);

            // check if there are new connection notifications where there is a single notification for multiple connections
            IEnumerable<IWebElement> notificationWithMultipleConnections = newNotifications.Where(n => 
            {
                IWebElement multipleNotification = default;
                try
                {
                    multipleNotification = n.FindElement(By.XPath(".//a//span[contains(text(), 'other accepted your invitations to connect')]")) ?? n.FindElement(By.XPath("//a//span[contains(text(), 'others accepted your invitations to connect')]"));
                }
                catch (Exception ex)
                {
                }
                return multipleNotification != null;
            });

            if(notificationWithMultipleConnections.Count() > 0)
            {
                // we've got new notifications that contain multiple accepted connections in one.
                // Navigate to each one and extract the connections name
                foreach (IWebElement multipleProspectsNotification in notificationWithMultipleConnections.ToList())
                {
                    newlyAcceptedProspectNames.AddRange(GetProspectInfoFromSingleNotification(multipleProspectsNotification, webDriver, timeZoneId));
                }
            }

            // find all notifications that include 'accepted your invitation to connect' text
            IEnumerable<IWebElement> newConnectionNotifications = newNotifications.Where(n =>
            {
                IWebElement newConnectionNotification = default;
                try
                {
                    newConnectionNotification = n.FindElement(By.XPath(".//a//span[contains(text(), 'accepted your invitation to connect')]"));
                }
                catch (Exception ex)
                {                    
                }
                return newConnectionNotification != null;
            });

            if(newConnectionNotifications.Count() > 0)
            {
                // get prospect name from each notification
                foreach (IWebElement prospectNotification in newConnectionNotifications)
                {
                    NewProspectConnectionRequest newProspectRequest = GetProspectInfoFromNotification(prospectNotification);
                    if(newProspectRequest != null)
                    {
                        newlyAcceptedProspectNames.Add(newProspectRequest);
                    }                    
                }
            }

            return newlyAcceptedProspectNames;
        }

        private IList<NewProspectConnectionRequest> GetProspectInfoFromSingleNotification(IWebElement notification, IWebDriver webDriver, string timeZoneId)
        {
            IList<NewProspectConnectionRequest> prospectsInfo = new List<NewProspectConnectionRequest>();
            IWebElement notificationAnchor = default;
            string notificationAnchorHref = string.Empty;
            try
            {
                // first locate the anchor tag element
                notificationAnchor = notification.FindElement(By.XPath(".//a"));
                notificationAnchorHref = notificationAnchor.GetAttribute("href");
            }
            catch(Exception ex)
            {
                _logger.LogWarning(ex, "Could not locate notification anchor. The notification achnor is the link which will be used to navigate to 'Accepted invitations' view. This is where all of the prospect names for this notification can be located and extracted.");
                return null;
            }

            RandomWait(2, 4);

            string mainWindowHandle = webDriver.CurrentWindowHandle;
            HalOperationResult<IOperationResponse> result = _webDriverProvider.NewTab<IOperationResponse>(webDriver);
            if (result.Succeeded == false)
            {
                return null;
            }

            webDriver.Navigate().GoToUrl(notificationAnchorHref);

            // now gather prospects names
            prospectsInfo = _acceptedInvitiationsView.GetAllProspectsInfo(webDriver, timeZoneId);

            _webDriverProvider.CloseTab<IOperationResponse>(BrowserPurpose.MonitorForNewAcceptedConnections, webDriver.CurrentWindowHandle);
            _webDriverProvider.SwitchTo<IOperationResponse>(webDriver, mainWindowHandle);

            return prospectsInfo;
            
        }

        private NewProspectConnectionRequest GetProspectInfoFromNotification(IWebElement notification)
        {
            NewProspectConnectionRequest newProspectInfo = new();
            IWebElement strongElement = default;
            try
            {
                strongElement = notification.FindElement(By.XPath(".//a//span[contains(text(), 'accepted your invitation to connect.')]//strong"));
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Failed to locate new prospects name in the notification");
            }
            
            if (strongElement != null)
            {
                newProspectInfo.ProspectName = strongElement.Text;
            }

            IWebElement profileUrl = default;
            try
            {
                profileUrl = notification.FindElement(By.XPath(".//a//span[contains(text(), 'accepted your invitation to connect.')]/parent::span/parent::a"));
            }
            catch(Exception ex)
            {
                _logger.LogWarning("Failed to locate new prospects profile url");
            }

            if (profileUrl != null)
            {
                newProspectInfo.ProfileUrl = profileUrl.GetAttribute("href");
            }

            if(newProspectInfo.ProspectName == null || newProspectInfo.ProfileUrl == null)
            {
                return null;
            }

            return newProspectInfo;
        }
    }
}
