using Domain.Services.Interfaces;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns.MonitorForNewProspects;
using Leadsly.Application.Model.Responses;
using Leadsly.Application.Model.Responses.Hal;
using Leadsly.Application.Model.Responses.Hal.Interfaces;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Services
{
    public class LinkedInHtmlParser : ILinkedInHtmlParser
    {
        public LinkedInHtmlParser(ILogger<LinkedInHtmlParser> logger)
        {
            _logger = logger;
        }

        private readonly ILogger<LinkedInHtmlParser> _logger;

        #region MonitorForNewConnections

        public HalOperationResult<T> ParseMyNetworkConnections<T>(IReadOnlyCollection<IWebElement> myNetworkNewConnections) where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            INewConnectionProspects newProspects = new NewConnectionProspects();
            foreach (IWebElement connection in myNetworkNewConnections)
            {
                NewConnectionProspect newProsp = new()
                {
                    NewConnectionInnerText = ExtractNewConnectionInnerText(connection),
                    ProfileUrl = ExtractProfileUrl(connection),
                    SmallProfilePhotoUrl = ExtractProfileUrl(connection)
                };
                newProspects.NewProspects.Add(newProsp);
            }

            result.Value = (T)newProspects;
            result.Succeeded = true;
            return result;
        }

        private string ExtractProfileUrl(IWebElement newConn)
        {
            string profileUrl = string.Empty;
            try
            {
                IWebElement profileUrlAnchor = newConn.FindElement(By.CssSelector("a"));
                if(profileUrlAnchor == null)
                {
                    _logger.LogWarning("Unable to locate profile url anchor tag. Used CssSelector 'a'. Returning empty string");
                    return profileUrl;
                }
                profileUrl = profileUrlAnchor.GetAttribute("href");

            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error occured locating anchor tag.");
            }
            return profileUrl;
        }

        private string ExtractProfileAvatarUrl(IWebElement newConn)
        {
            string profileAvatarUrl = string.Empty;
            try
            {
                IWebElement profileAvatarUrlElement = newConn.FindElement(By.CssSelector("a"));
                if (profileAvatarUrlElement == null)
                {
                    _logger.LogWarning("Unable to locate profile url anchor tag. Used CssSelector 'a'. Returning empty string");
                    return profileAvatarUrl;
                }
                IWebElement imageTag = profileAvatarUrlElement.FindElement(By.CssSelector("a > img"));
                if(imageTag == null)
                {
                    _logger.LogWarning("Unable locate image tag inside the anchor tag. Returning empty string");
                }
                profileAvatarUrl = imageTag.GetAttribute("src");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to extract src value for user's avatar url");
            }
            return profileAvatarUrl;
        }
        private string ExtractNewConnectionInnerText(IWebElement newConn)
        {
            string innerText = string.Empty;
            try
            {
                IWebElement spanElement = newConn.FindElement(By.CssSelector("div > span"));
                if(spanElement == null)
                {
                    _logger.LogWarning("Unable to locate span which contains user's first name and 'accepted your invitation to connect' text");
                }
                innerText = spanElement.Text;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to extract inner text on new connection. This inner text contains user's first name");
            }
            return innerText;
        }

        #endregion
    }
}
