using Domain.POMs;
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

        public IList<IWebElement> GetAllProspects(IWebDriver webDriver)
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

        public IList<string> GetAllProspectsNames(IWebDriver webDriver)
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
    }
}
