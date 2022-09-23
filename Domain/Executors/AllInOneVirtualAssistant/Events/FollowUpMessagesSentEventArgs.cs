using Domain.Models.FollowUpMessage;
using Domain.MQ.Messages;
using System;
using System.Collections.Generic;

namespace Domain.Executors.AllInOneVirtualAssistant.Events
{
    public class FollowUpMessagesSentEventArgs : EventArgs
    {
        public FollowUpMessagesSentEventArgs(PublishMessageBody message, IList<SentFollowUpMessageModel> sentFollowUpMessages)
        {
            Message = message;
            SentFollowUpMessages = sentFollowUpMessages;
        }

        public PublishMessageBody Message { get; set; }
        public IList<SentFollowUpMessageModel> SentFollowUpMessages { get; set; }

    }
}
