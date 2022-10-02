﻿using Domain.Executors.AllInOneVirtualAssistant.Events;
using Domain.Models.FollowUpMessage;
using Domain.MQ.Messages;
using OpenQA.Selenium;

namespace Domain.InstructionSets.Interfaces
{
    public interface IFollowUpMessageInstructionSet
    {
        public event ProspectsThatRepliedEventHandler ProspectsThatRepliedDetected;
        public SentFollowUpMessageModel GetSentFollowUpMessage();
        public void SendFollowUpMessage(IWebDriver webDriver, FollowUpMessageBody message);
        public void SendFollowUpMessage_AllInOne(IWebDriver webDriver, FollowUpMessageBody message);
    }
}
