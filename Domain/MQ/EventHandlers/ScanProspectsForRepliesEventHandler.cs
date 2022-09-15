using Domain.Models.RabbitMQMessages;
using Domain.MQ.EventHandlers.Interfaces;
using Domain.PhaseHandlers.ScanProspectsForRepliesHandler;
using Leadsly.Application.Model;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Domain.MQ.EventHandlers
{
    public class ScanProspectsForRepliesEventHandler : RabbitMQEventHandlerBase, IScanProspectsForRepliesEventHandler
    {
        public ScanProspectsForRepliesEventHandler(
            ILogger<ScanProspectsForRepliesEventHandler> logger,
            HalWorkCommandHandlerDecorator<DeepScanProspectsForRepliesCommand> deepScanHandler,
            HalWorkCommandHandlerDecorator<ScanProspectsForRepliesCommand> scanHandler
            ) : base(logger)
        {
            _logger = logger;
            _deepScanHandler = deepScanHandler;
            _scanHandler = scanHandler;
        }

        private readonly HalWorkCommandHandlerDecorator<DeepScanProspectsForRepliesCommand> _deepScanHandler;
        private readonly HalWorkCommandHandlerDecorator<ScanProspectsForRepliesCommand> _scanHandler;
        private readonly ILogger<ScanProspectsForRepliesEventHandler> _logger;

        public async Task OnScanProspectsForRepliesEventReceivedAsync(object sender, BasicDeliverEventArgs eventArgs)
        {
            IModel channel = ((AsyncEventingBasicConsumer)sender).Model;

            var headers = eventArgs.BasicProperties.Headers;
            headers.TryGetValue(RabbitMQConstants.ScanProspectsForReplies.ExecutionType, out object networkTypeObj);

            byte[] networkTypeArr = networkTypeObj as byte[];

            string networkType = Encoding.UTF8.GetString(networkTypeArr);
            if (networkType == null)
            {
                _logger.LogError("Failed to determine execution type for ScanProspectsForRepliesPhase. It should be a header either for 'ExecuteOnce' or 'ExecutePhase'. " +
                    "ExecuteOnce is to check off hours responses and ExecutePhase is for round the clock phase");
                throw new ArgumentException("A header value must be provided!");
            }

            byte[] body = eventArgs.Body.ToArray();
            string rawMessage = Encoding.UTF8.GetString(body);

            if (networkType == RabbitMQConstants.ScanProspectsForReplies.ExecuteDeepScan)
            {
                PublishMessageBody message = DeserializeMessage<DeepScanProspectsForRepliesBody>(rawMessage);
                DeepScanProspectsForRepliesCommand deepScanProspectsCommand = new DeepScanProspectsForRepliesCommand(channel, eventArgs, message, message.StartOfWorkday, message.EndOfWorkday, message.TimeZoneId);
                await _deepScanHandler.HandleAsync(deepScanProspectsCommand);
            }
            else if (networkType == RabbitMQConstants.ScanProspectsForReplies.ExecutePhase)
            {
                PublishMessageBody message = DeserializeMessage<ScanProspectsForRepliesBody>(rawMessage);
                ScanProspectsForRepliesCommand scanProspectsCommand = new ScanProspectsForRepliesCommand(channel, eventArgs, message, message.StartOfWorkday, message.EndOfWorkday, message.TimeZoneId);
                await _scanHandler.HandleAsync(scanProspectsCommand);
            }
        }
    }
}
