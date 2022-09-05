using Domain.POMs;
using Domain.POMs.Pages;

namespace Domain.Facades.Interfaces
{
    public interface ILinkedInPageFacade
    {
        public ILinkedInHomePage LinkedInHomePage { get; }
        public ILinkedInLoginPage LinkedInLoginPage { get; }
        public ILinkedInMessagingPage LinkedInMessagingPage { get; }
        public ILinkedInPage LinkedInPage { get; }
        public IConnectionsView ConnectionsView { get; }
        public ILinkedInSearchPage LinkedInSearchPage { get; }
    }
}
