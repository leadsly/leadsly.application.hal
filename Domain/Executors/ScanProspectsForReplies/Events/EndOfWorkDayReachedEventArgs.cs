using Domain.MQ.Messages;
using System;

namespace Domain.Executors.ScanProspectsForReplies.Events
{
    public class EndOfWorkDayReachedEventArgs : EventArgs
    {
        public EndOfWorkDayReachedEventArgs(PublishMessageBody message)
        {
            Message = message;
        }

        public PublishMessageBody Message { get; set; }
    }
}
