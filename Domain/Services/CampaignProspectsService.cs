using Domain.Services.Interfaces;
using Leadsly.Application.Model.Requests.FromHal;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Services
{
    public class CampaignProspectsService : ICampaignProspectsService
    {
        public CampaignProspectsService(ILogger<CampaignProspectsService> logger)
        {
            _logger = logger;
        }

        private readonly ILogger<CampaignProspectsService> _logger;

        public CampaignProspectRequest CreateCampaignProspects(IWebElement prospect, string campaignId)
        {
            return new CampaignProspectRequest()
            {
                ConnectionSentTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds(),
                Name = GetProspectsName(prospect),
                ProfileUrl = GetProspectsProfileUrl(prospect),
                CampaignId = campaignId
            };
        }

        private string GetProspectsName(IWebElement webElement)
        {
            string prospectName = string.Empty;
            try
            {
                IWebElement prospectNameSpan = webElement.FindElement(By.CssSelector(".entity-result__title-text"));
                try
                {
                    IWebElement thirdConnectionProspectName = prospectNameSpan.FindElement(By.CssSelector("span[aria-hidden=true]"));
                    prospectName = thirdConnectionProspectName.Text;
                }
                catch (Exception ex)
                {
                    // ignore the error and proceed
                    prospectName = prospectNameSpan.Text;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Webdriver error occured extracting prospects name");
            }

            return prospectName;
        }

        private string GetProspectsProfileUrl(IWebElement webElement)
        {
            string[] innerText = webElement.Text.Split("\r\n");
            string userName = innerText[0] ?? string.Empty;
            if (userName == "LinkedIn Member")
            {
                // this means we don't have access to user's profile
                return string.Empty;
            }

            string profileUrl = string.Empty;
            try
            {
                IWebElement anchorTag = webElement.FindElement(By.CssSelector(".app-aware-link"));
                profileUrl = anchorTag.GetAttribute("href");
                profileUrl = profileUrl.Split('?').FirstOrDefault();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Webdriver error occured extracting prospects profile url");
            }

            return profileUrl;
        }
    }
}
