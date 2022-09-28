using Domain.MQ.Messages;
using System;

namespace Domain.Executors.AllInOneVirtualAssistant.Events
{
    public class MonthlySearchLimitReachedEventArgs : EventArgs
    {
        public MonthlySearchLimitReachedEventArgs(PublishMessageBody message, bool limitReached)
        {
            Message = message;
            LimitReached = limitReached;
        }

        public PublishMessageBody Message { get; set; }
        public bool LimitReached { get; set; }

    }
}
