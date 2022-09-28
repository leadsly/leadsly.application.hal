using Domain.Models.SendConnections;
using Domain.MQ.Messages;
using System;
using System.Collections.Generic;

namespace Domain.Executors.AllInOneVirtualAssistant.Events
{
    public class ConnectionsSentEventArgs : EventArgs
    {
        public ConnectionsSentEventArgs(PublishMessageBody message, IList<ConnectionSentModel> connectionsSent)
        {
            Message = message;
            ConnectionsSent = connectionsSent;
        }

        public PublishMessageBody Message { get; set; }
        public IList<ConnectionSentModel> ConnectionsSent { get; set; }

    }
}
