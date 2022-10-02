using Domain.Executors.AllInOneVirtualAssistant.Events;
using Domain.Models.FollowUpMessage;
using Domain.MQ.Messages;
using OpenQA.Selenium;
using System.Collections.Generic;

namespace Domain.Orchestrators.Interfaces
{
    public interface IFollowUpMessagePhaseOrchestrator
    {
        public event FollowUpMessagesSentEventHandler FollowUpMessagesSent;
        public event ProspectsThatRepliedEventHandler ProspectsThatRepliedDetected;
        public SentFollowUpMessageModel GetSentFollowUpMessage();
        public IList<SentFollowUpMessageModel> GetSentFollowUpMessages();
        public void Execute(FollowUpMessageBody message);
        public void Execute(IWebDriver webDriver, AllInOneVirtualAssistantMessageBody message);
    }
}
