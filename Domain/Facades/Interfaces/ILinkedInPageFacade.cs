using Domain.POMs;
using Domain.POMs.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Facades.Interfaces
{
    public interface ILinkedInPageFacade
    {
        public ILinkedInHomePage LinkedInHomePage { get; }
        public ILinkedInLoginPage LinkedInLoginPage { get; }
        public ILinkedInMessagingPage LinkedInMessagingPage { get; }
        public ILinkedInMyNetworkPage LinkedInMyNetworkPage { get; }
        public ILinkedInNotificationsPage LinkedInNotificationsPage { get; }
        public ILinkedInPage LinkedInPage { get; }
        public IConnectionsView ConnectionsView { get; }
        public ILinkedInSearchPage LinkedInSearchPage { get; }
        public IAcceptedInvitiationsView AccepatedInvitationsView { get; }
        public ILinkedInNavBar LinkedInNavBar { get;  }
    }
}
