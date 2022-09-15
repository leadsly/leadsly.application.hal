using Domain.MQ.Messages;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.PhaseHandlers.FollowUpMessageHandlers
{
    public class FollowUpMessageCommand : ICommand
    {
        public FollowUpMessageCommand(IModel channel, BasicDeliverEventArgs eventArgs, PublishMessageBody messageBody, string startOfWorkDay, string endOfWorkday, string timezoneId)
        {
            Channel = channel;
            EventArgs = eventArgs;
            MessageBody = messageBody;
            StartOfWorkDay = startOfWorkDay;
            EndOfWorkDay = endOfWorkday;
            TimeZoneId = timezoneId;
        }

        public IModel Channel { get; set; }
        public BasicDeliverEventArgs EventArgs { get; set; }
        public PublishMessageBody MessageBody { get; private set; }
        public string StartOfWorkDay { get; set; }
        public string EndOfWorkDay { get; set; }
        public string TimeZoneId { get; set; }
    }
}
