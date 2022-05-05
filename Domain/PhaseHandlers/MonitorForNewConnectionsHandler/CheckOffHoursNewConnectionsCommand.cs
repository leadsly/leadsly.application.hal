using Leadsly.Application.Model.Campaigns;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.PhaseHandlers.MonitorForNewConnectionsHandler
{
    public class CheckOffHoursNewConnectionsCommand : ICommand
    {
        public CheckOffHoursNewConnectionsCommand(IModel channel, BasicDeliverEventArgs eventArgs, PublishMessageBody messageBody, string startOfDay, string endOfDay, string timeZoneId)
        {
            Channel = channel;
            EventArgs = eventArgs;
            MessageBody = messageBody;
            StartOfWorkDay = startOfDay;
            EndOfWorkDay = endOfDay;
            TimeZoneId = timeZoneId;
        }
        public IModel Channel { get; set; }
        public BasicDeliverEventArgs EventArgs { get; set; }
        public PublishMessageBody MessageBody { get; private set; }
        public string StartOfWorkDay { get; set; }
        public string EndOfWorkDay { get; set; }
        public string TimeZoneId { get; set; }
    }
}
