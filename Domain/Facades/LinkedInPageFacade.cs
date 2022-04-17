using Domain.Facades.Interfaces;
using Domain.POMs;
using Domain.POMs.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Facades
{
    public class LinkedInPageFacade : ILinkedInPageFacade
    {
        public LinkedInPageFacade(
            ILinkedInHomePage linkedInHomePage,
            ILinkedInLoginPage linkedInLoginPage,
            ILinkedInMessagingPage linkedInMessagingPage,
            ILinkedInMyNetworkPage linkedInMyNetworkPage,
            ILinkedInNotificationsPage linkedInNotificationsPage,
            ILinkedInPage linkedInPage,
            ILinkedInSearchPage linkedInSearchPage,
            IAcceptedInvitiationsView acceptedInvitiationsView,
            ILinkedInNavBar linkedInNavBar
            )
        {
            LinkedInHomePage = linkedInHomePage;
            LinkedInLoginPage = linkedInLoginPage;
            LinkedInMessagingPage = linkedInMessagingPage;
            LinkedInMyNetworkPage = linkedInMyNetworkPage;
            LinkedInNotificationsPage = linkedInNotificationsPage;
            LinkedInSearchPage = linkedInSearchPage;
            LinkedInPage = linkedInPage;
            AccepatedInvitationsView = acceptedInvitiationsView;
            LinkedInNavBar = linkedInNavBar;
        }

        public ILinkedInHomePage LinkedInHomePage { get; }

        public ILinkedInLoginPage LinkedInLoginPage { get; }

        public ILinkedInMessagingPage LinkedInMessagingPage { get; }

        public ILinkedInMyNetworkPage LinkedInMyNetworkPage { get; }

        public ILinkedInNotificationsPage LinkedInNotificationsPage { get; }

        public ILinkedInPage LinkedInPage { get; }

        public ILinkedInSearchPage LinkedInSearchPage { get; }

        public IAcceptedInvitiationsView AccepatedInvitationsView { get; }

        public ILinkedInNavBar LinkedInNavBar { get; }
    }
}
