using Domain.Models.RabbitMQMessages;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.PhaseHandlers.ScanProspectsForRepliesHandler
{
    public class DeepScanProspectsForRepliesCommand : ICommand
    {
        public DeepScanProspectsForRepliesCommand(IModel channel, BasicDeliverEventArgs eventArgs, PublishMessageBody messageBody, string startOfday, string endOfDay, string timeZoneId)
        {
            Channel = channel;
            EventArgs = eventArgs;
            MessageBody = messageBody;
            StartOfWorkDay = startOfday;
            EndOfWorkDay = endOfDay;
            TimeZoneId = timeZoneId;
        }

        public IModel Channel { get; set; }
        public BasicDeliverEventArgs EventArgs { get; set; }
        public PublishMessageBody MessageBody { get; set; }
        public string StartOfWorkDay { get; set; }
        public string EndOfWorkDay { get; set; }
        public string TimeZoneId { get; set; }
    }
}
