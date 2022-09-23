using Domain.Models.DeepScanProspectsForReplies;
using Domain.Models.Networking;
using OpenQA.Selenium;
using System.Collections.Generic;

namespace Domain.InstructionSets.Interfaces
{
    public interface IDeepScanInstructionSet
    {
        IList<ProspectRepliedModel> Prospects { get; }
        int VisibleConversationCount { get; }
        void BeginDeepScanning(IWebDriver webDriver, IList<NetworkProspectModel> prospects, int visibleMessagesCount);
        bool ClearMessagingSearchCriteriaInteraction(IWebDriver webDriver);
        bool GetVisibleConversationCountInteraction(IWebDriver webDriver);
    }
}
