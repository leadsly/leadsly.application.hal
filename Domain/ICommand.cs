using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Domain
{
    public interface ICommand
    {
        public IModel Channel { get; }
        public BasicDeliverEventArgs EventArgs { get; }
        public string StartOfWorkDay { get; }
        public string EndOfWorkDay { get; }
        public string TimeZoneId { get; }
    }
}
