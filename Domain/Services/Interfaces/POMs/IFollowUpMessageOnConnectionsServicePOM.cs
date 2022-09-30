using OpenQA.Selenium;
using System.Collections.Generic;

namespace Domain.Services.Interfaces.POMs
{
    public interface IFollowUpMessageOnConnectionsServicePOM
    {
        public IList<IWebElement> Messages { get; }
        IWebElement GetProspectFromRecentlyAdded(IWebDriver webDriver, string prospectName);
        bool ClickMessageProspect(IWebDriver webDriver, IWebElement prospect);
        bool IsThereConversationHistory(IWebElement conversation);
        IWebElement GetPopUpConversation(IWebDriver webDriver, string prospectNameIn);
        bool? WasLastMessageSentByProspect(IWebElement lastMessage, string prospectName);
        bool? DoesLastMessageMatchPreviouslySentMessage(IWebElement lastMessage, string previousMessageContent);
        bool EnterMessage(IWebDriver webDriver, IWebElement conversationPopUp, string content);
    }
}
