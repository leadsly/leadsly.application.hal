using Leadsly.Application.Model.Campaigns;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
