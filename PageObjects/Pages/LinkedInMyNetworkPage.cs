using Domain.POMs;
using Domain.POMs.Pages;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Responses;
using Leadsly.Application.Model.Responses.Hal.Interfaces;
using Leadsly.Application.Model.Responses.Hal.POMs;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PageObjects.Pages
{
    public class LinkedInMyNetworkPage : ILinkedInMyNetworkPage
    {
        public LinkedInMyNetworkPage(ILogger<LinkedInMyNetworkPage> logger)
        {
            _logger = logger;
        }

        private readonly ILogger<LinkedInMyNetworkPage> _logger;

        public HalOperationResult<T> CollectAllNewConnections<T>(IWebDriver webDriver, int newConnectionCount) where T : IOperationResponse
        {
            HalOperationResult<T> result = new();

            IReadOnlyCollection<IWebElement> newInvitations = NewInvitations(webDriver);
            if(newInvitations == null)
            {
                result.Failures.Add(new()
                {
                    Detail = "Failed to collect new connections",
                    Reason = "Unable to locate new connections. Maybe the css selector needs updating?"
                });
                return result;
            }

            IEnumerable<IWebElement> newConnections = newInvitations.Take(newConnectionCount);

            INewInvitationsMyNetwork newNetworkInvitations = new NewInvitationsMyNetwork
            {
                NewConnections = newConnections.ToList()
            };

            result.Value = (T)newNetworkInvitations;
            result.Succeeded = true;
            return result;
        }

        private IReadOnlyCollection<IWebElement> NewInvitations(IWebDriver webDriver)
        {
            IReadOnlyCollection<IWebElement> newConnections = default;
            try
            {
                 newConnections = webDriver.FindElements(By.CssSelector("#ember2199 section .align-items-center"));
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Failed to get all new connections");
            }

            return newConnections;
        }

    }
}
