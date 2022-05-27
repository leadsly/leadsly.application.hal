using Leadsly.Application.Model.Campaigns;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.PhaseHandlers.ProspectListHandler
{
    public class ProspectListCommand : ICommand
    {
        public ProspectListCommand(IModel channel, BasicDeliverEventArgs eventArgs, PublishMessageBody messageBody, string startOfWorkday, string endofWorkday, string timezoneId)
        {
            Channel = channel;
            EventArgs = eventArgs;
            StartOfWorkDay = startOfWorkday;
            EndOfWorkDay = endofWorkday;
            TimeZoneId = timezoneId;
            MessageBody = messageBody;
        }

        public PublishMessageBody MessageBody { get; set; }
        public string StartOfWorkDay { get; set; }
        public string EndOfWorkDay { get; set; }
        public string TimeZoneId { get; set; }
        public IModel Channel { get; set; }
        public BasicDeliverEventArgs EventArgs { get; set; }
    }
}
