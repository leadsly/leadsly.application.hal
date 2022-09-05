using Domain.Models.RabbitMQMessages;
using Domain.PhaseHandlers.ScanProspectsForRepliesHandler;
using Domain.RabbitMQ.EventHandlers.Interfaces;
using Leadsly.Application.Model;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Domain.RabbitMQ.EventHandlers
{
    public class ScanProspectsForRepliesEventHandler : RabbitMQEventHandlerBase, IScanProspectsForRepliesEventHandler
    {
        public ScanProspectsForRepliesEventHandler(
            ILogger<ScanProspectsForRepliesEventHandler> logger,
            HalWorkCommandHandlerDecorator<DeepScanProspectsForRepliesCommand> deepScanHandler,
            HalWorkCommandHandlerDecorator<ScanProspectsForRepliesCommand> scanHandler
            )
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
                PublishMessageBody message = DeserializeMessage(rawMessage);
                DeepScanProspectsForRepliesCommand deepScanProspectsCommand = new DeepScanProspectsForRepliesCommand(channel, eventArgs, message, message.StartOfWorkday, message.EndOfWorkday, message.TimeZoneId);
                await _deepScanHandler.HandleAsync(deepScanProspectsCommand);
            }
            else if (networkType == RabbitMQConstants.ScanProspectsForReplies.ExecutePhase)
            {
                PublishMessageBody message = DeserializeMessage(rawMessage);
                ScanProspectsForRepliesCommand scanProspectsCommand = new ScanProspectsForRepliesCommand(channel, eventArgs, message, message.StartOfWorkday, message.EndOfWorkday, message.TimeZoneId);
                await _scanHandler.HandleAsync(scanProspectsCommand);
            }
        }

        protected override PublishMessageBody DeserializeMessage(string rawMessage)
        {
            _logger.LogInformation("Deserializing DeepScanProspectsForRepliesBody");
            PublishMessageBody message = null;
            try
            {
                message = JsonConvert.DeserializeObject<PublishMessageBody>(rawMessage);
                _logger.LogDebug("Successfully deserialized DeepScanProspectsForRepliesBody");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deserialize DeepScanProspectsForRepliesBody. Returning an explicit null");
                return null;
            }

            return message;
        }
    }
}
