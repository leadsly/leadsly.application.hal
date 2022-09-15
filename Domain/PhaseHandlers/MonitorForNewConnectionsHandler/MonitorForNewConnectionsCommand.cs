using Domain.MQ.Messages;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.PhaseHandlers.MonitorForNewConnectionsHandler
{
    public class MonitorForNewConnectionsCommand : ICommand
    {
        public MonitorForNewConnectionsCommand(IModel channel, BasicDeliverEventArgs eventArgs, PublishMessageBody messageBody, string startOfWorkDay, string endOfWorkDay, string timezoneId)
        {
            Channel = channel;
            EventArgs = eventArgs;
            MessageBody = messageBody;
            StartOfWorkDay = startOfWorkDay;
            EndOfWorkDay = endOfWorkDay;
            TimeZoneId = timezoneId;
        }

        public IModel Channel { get; set; }
        public BasicDeliverEventArgs EventArgs { get; set; }
        public PublishMessageBody MessageBody { get; set; }
        public string StartOfWorkDay { get; set; }
        public string EndOfWorkDay { get; set; }
        public string TimeZoneId { get; set; }
    }
}
