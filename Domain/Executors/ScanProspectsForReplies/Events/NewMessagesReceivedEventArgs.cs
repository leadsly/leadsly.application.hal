using Domain.Models.RabbitMQMessages;
using Domain.Models.ScanProspectsForReplies;
using System;
using System.Collections.Generic;

namespace Domain.Executors.ScanProspectsForReplies.Events
{
    public class NewMessagesReceivedEventArgs : EventArgs
    {
        public NewMessagesReceivedEventArgs(PublishMessageBody message, IList<NewMessageModel> newMessages)
        {
            NewMessages = newMessages;
            Message = message;
        }
        public IList<NewMessageModel> NewMessages { get; }
        public PublishMessageBody Message { get; set; }
    }
}
