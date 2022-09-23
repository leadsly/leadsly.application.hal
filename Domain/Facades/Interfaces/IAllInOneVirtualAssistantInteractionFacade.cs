using Domain.Interactions;
using Domain.Models.MonitorForNewProspects;
using Domain.Models.ScanProspectsForReplies;
using OpenQA.Selenium;
using System.Collections.Generic;

namespace Domain.Facades.Interfaces
{
    public interface IAllInOneVirtualAssistantInteractionFacade
    {
        public int TotalNumberOfSearchResults { get; }
        public int ConnectionsCount { get; }
        public IList<IWebElement> UnreadMessages { get; }
        public IList<NewMessageModel> NewMessages { get; }
        public IList<RecentlyAddedProspectModel> RecentlyAddedProspects { get; }
        bool HandleGetConnectionsCountInteraction(InteractionBase interaction);
        bool HandleGetAllRecentlyAddedInteraction(InteractionBase interaction);
        bool HandleGetAllUnreadMessageListBubbles(InteractionBase interaction);
        bool HandleGetUnreadMessagesContent(InteractionBase interaction);
    }
}
