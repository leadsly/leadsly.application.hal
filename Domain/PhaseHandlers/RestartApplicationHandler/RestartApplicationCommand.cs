using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Domain.PhaseHandlers.RestartApplicationHandler
{
    public class RestartApplicationCommand : ICommand
    {
        public RestartApplicationCommand(IModel channel, BasicDeliverEventArgs eventArgs)
        {
            Channel = channel;
            EventArgs = eventArgs;
        }

        public IModel Channel { get; private set; }
        public BasicDeliverEventArgs EventArgs { get; private set; }
        public string StartOfWorkDay { get; private set; }
        public string EndOfWorkDay { get; private set; }
        public string TimeZoneId { get; private set; }
    }
}
