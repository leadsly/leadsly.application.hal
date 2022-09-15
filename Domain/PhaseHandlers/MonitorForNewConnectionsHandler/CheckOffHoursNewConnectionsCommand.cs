using Domain.MQ.Messages;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Domain.PhaseHandlers.MonitorForNewConnectionsHandler
{
    public class CheckOffHoursNewConnectionsCommand : ICommand
    {
        public CheckOffHoursNewConnectionsCommand(IModel channel, BasicDeliverEventArgs eventArgs, PublishMessageBody messageBody)
        {
            Channel = channel;
            EventArgs = eventArgs;
            MessageBody = messageBody;
        }
        public IModel Channel { get; set; }
        public BasicDeliverEventArgs EventArgs { get; set; }
        public PublishMessageBody MessageBody { get; private set; }

        public string StartOfWorkDay => throw new System.NotImplementedException();

        public string EndOfWorkDay => throw new System.NotImplementedException();

        public string TimeZoneId => throw new System.NotImplementedException();
    }
}
