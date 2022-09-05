using Domain.Facades.Interfaces;
using Domain.POMs;
using Domain.POMs.Pages;

namespace Domain.Facades
{
    public class LinkedInPageFacade : ILinkedInPageFacade
    {
        public LinkedInPageFacade(
            ILinkedInHomePage linkedInHomePage,
            ILinkedInLoginPage linkedInLoginPage,
            ILinkedInMessagingPage linkedInMessagingPage,
            ILinkedInPage linkedInPage,
            ILinkedInSearchPage linkedInSearchPage,
            IConnectionsView connectionsView
            )
        {
            LinkedInHomePage = linkedInHomePage;
            LinkedInLoginPage = linkedInLoginPage;
            LinkedInMessagingPage = linkedInMessagingPage;
            LinkedInSearchPage = linkedInSearchPage;
            LinkedInPage = linkedInPage;
            ConnectionsView = connectionsView;
        }

        public ILinkedInHomePage LinkedInHomePage { get; }

        public ILinkedInLoginPage LinkedInLoginPage { get; }

        public ILinkedInMessagingPage LinkedInMessagingPage { get; }

        public ILinkedInPage LinkedInPage { get; }

        public ILinkedInSearchPage LinkedInSearchPage { get; }

        public IConnectionsView ConnectionsView { get; }
    }
}
