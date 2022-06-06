using Domain;
using Domain.POMs;
using Leadsly.Application.Model.Requests.FromHal;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PageObjects
{
    public class AcceptedInvitationsView : IAcceptedInvitiationsView
    {
        public AcceptedInvitationsView(ILogger<AcceptedInvitationsView> logger)
        {
            _logger = logger;
        }

        private readonly ILogger<AcceptedInvitationsView> _logger;

        private IList<IWebElement> GetAllProspects(IWebDriver webDriver)
        {
            IList<IWebElement> prospects = new List<IWebElement>();
            try
            {
                prospects = webDriver.FindElements(By.CssSelector(".nt-card")).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get all prospects from accepted invitations list view");
            }
            return prospects;
        }

        private IList<string> GetAllProspectsNames(IWebDriver webDriver)
        {
            IList<IWebElement> prospects = GetAllProspects(webDriver);
            if(prospects == null)
            {
                return null;
            }

            IList<string> names = new List<string>();
            foreach (IWebElement prospect in prospects)
            {
                IWebElement strongElement = prospect.FindElement(By.XPath("//article[contains(@class, 'nt-card')]//a//span[contains(text(), 'accepted your invitation to connect.')]//strong"));
                if(strongElement != null)
                {
                    names.Add(strongElement.Text);
                }
            }

            return names;
        }

        public IList<NewProspectConnectionRequest> GetAllProspectsInfo(IWebDriver webDriver, string timeZoneId)
        {
            IList<IWebElement> prospects = GetAllProspects(webDriver);
            if(prospects == null)
            {
                return null;
            }

            IList<NewProspectConnectionRequest> prospectsInfo = new List<NewProspectConnectionRequest>();
            foreach (IWebElement prospect in prospects)
            {
                NewProspectConnectionRequest newProspectInfo = new();
                IWebElement strongElement = default;
                try
                {
                    strongElement = prospect.FindElement(By.XPath(".//a//span[contains(text(), 'accepted your invitation to connect.')]//strong"));
                }
                catch(Exception ex)
                {
                    _logger.LogWarning("Failed to locate new prospects name");
                }
                
                if (strongElement != null)
                {
                    newProspectInfo.ProspectName = strongElement.Text;
                }

                IWebElement profileUrl = default;
                try
                {
                    profileUrl = prospect.FindElement(By.XPath(".//a//span[contains(text(), 'accepted your invitation to connect.')]/parent::span/parent::a"));
                }
                catch(Exception ex)
                {
                    _logger.LogWarning("Failed to locate new prospects profile url");
                }
                                
                if(profileUrl != null)
                {
                    newProspectInfo.ProfileUrl = profileUrl.GetAttribute("href");
                }

                if(newProspectInfo.ProspectName != null && newProspectInfo.ProfileUrl != null)
                {
                    newProspectInfo.AcceptedTimestamp = new DateTimeOffset().ToUnixTimeSeconds();
                    prospectsInfo.Add(newProspectInfo);
                }
            }

            return prospectsInfo;
        }
    }
}
