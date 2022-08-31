using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.Requests;
using System;
using System.Collections.Generic;

namespace Domain.Executors.ScanProspectsForReplies.Events
{
    public class NewMessagesReceivedEventArgs : EventArgs
    {
        public NewMessagesReceivedEventArgs(PublishMessageBody message, IList<NewMessageRequest> newMessages)
        {
            NewMessages = newMessages;
            Message = message;
        }
        public IList<NewMessageRequest> NewMessages { get; }
        public PublishMessageBody Message { get; set; }
    }
}
