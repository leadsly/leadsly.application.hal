using Domain.PhaseHandlers.FollowUpMessageHandlers;
using Domain.PhaseHandlers.MonitorForNewConnectionsHandler;
using Domain.PhaseHandlers.NetworkingHandler;
using Domain.PhaseHandlers.ProspectListHandler;
using Domain.PhaseHandlers.RestartApplicationHandler;
using Domain.PhaseHandlers.ScanProspectsForRepliesHandler;
using Domain.PhaseHandlers.SendConnectionsHandler;
using Domain.Providers.Interfaces;
using Domain.Serializers.Interfaces;
using Domain.Services.Interfaces;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Services
{
    public class PhaseEventHandlerService : IPhaseEventHandlerService
    {
        public PhaseEventHandlerService(
            ILogger<PhaseEventHandlerService> logger,
            ICommandHandler<RestartApplicationCommand> restartAppHandler,
            HalWorkCommandHandlerDecorator<FollowUpMessageCommand> followUpHandler,
            HalWorkCommandHandlerDecorator<SendConnectionsCommand> sendConnectionsHandler,
            HalWorkCommandHandlerDecorator<ProspectListCommand> prospectListHandler,
            HalWorkCommandHandlerDecorator<MonitorForNewConnectionsCommand> monitorHandler,
            HalWorkCommandHandlerDecorator<ScanProspectsForRepliesCommand> scanHandler,
            HalWorkCommandHandlerDecorator<CheckOffHoursNewConnectionsCommand> offHoursHandler,
            HalWorkCommandHandlerDecorator<DeepScanProspectsForRepliesCommand> deepScanHandler,
            HalWorkCommandHandlerDecorator<NetworkingCommand> networkingHandler,
            IWebDriverProvider webDriverProvider,
            IRabbitMQSerializer serializer
            )
        {
            _logger = logger;
            _restartAppHandler = restartAppHandler;
            _networkingHandler = networkingHandler;
            _webDriverProvider = webDriverProvider;
            _followUpHandler = followUpHandler;
            _sendConnectionsHandler = sendConnectionsHandler;
            _monitorHandler = monitorHandler;
            _prospectListHandler = prospectListHandler;
            _scanHandler = scanHandler;
            _offHoursHandler = offHoursHandler;
            _deepScanHandler = deepScanHandler;
            _serializer = serializer;
        }

        private readonly ICommandHandler<RestartApplicationCommand> _restartAppHandler;
        private readonly HalWorkCommandHandlerDecorator<CheckOffHoursNewConnectionsCommand> _offHoursHandler;
        private readonly HalWorkCommandHandlerDecorator<DeepScanProspectsForRepliesCommand> _deepScanHandler;
        private readonly HalWorkCommandHandlerDecorator<ScanProspectsForRepliesCommand> _scanHandler;
        private readonly HalWorkCommandHandlerDecorator<MonitorForNewConnectionsCommand> _monitorHandler;
        private readonly HalWorkCommandHandlerDecorator<FollowUpMessageCommand> _followUpHandler;
        private readonly HalWorkCommandHandlerDecorator<SendConnectionsCommand> _sendConnectionsHandler;
        private readonly HalWorkCommandHandlerDecorator<ProspectListCommand> _prospectListHandler;
        private readonly HalWorkCommandHandlerDecorator<NetworkingCommand> _networkingHandler;
        private readonly ILogger<PhaseEventHandlerService> _logger;
        private readonly IWebDriverProvider _webDriverProvider;
        private readonly IRabbitMQSerializer _serializer;

        #region NetworkingConnections

        public async Task OnNetworkingConnectionsEventReceivedAsync(object sender, BasicDeliverEventArgs eventArgs)
        {
            IModel channel = ((AsyncEventingBasicConsumer)sender).Model;

            var headers = eventArgs.BasicProperties.Headers;
            headers.TryGetValue(RabbitMQConstants.NetworkingConnections.NetworkingType, out object networkTypeObj);

            byte[] networkTypeArr = networkTypeObj as byte[];
            if (networkTypeArr == null)
            {
                _logger.LogError("Failed to determine networking connection type. It should be a header either for SendConnectionRequests or for ProspectList");
                throw new ArgumentException("A header value must be provided!");
            }

            string networkType = Encoding.UTF8.GetString(networkTypeArr);
            if ((networkType as string) == RabbitMQConstants.NetworkingConnections.ProspectList)
            {
                byte[] body = eventArgs.Body.ToArray();
                string message = Encoding.UTF8.GetString(body);
                ProspectListBody prospectListBody = _serializer.DeserializeProspectListBody(message);

                ProspectListCommand prospectListCommand = new ProspectListCommand(channel, eventArgs, prospectListBody, prospectListBody.StartOfWorkday, prospectListBody.EndOfWorkday, prospectListBody.TimeZoneId);
                await _prospectListHandler.HandleAsync(prospectListCommand);
            }
            else if ((networkType as string) == RabbitMQConstants.NetworkingConnections.SendConnectionRequests)
            {
                byte[] body = eventArgs.Body.ToArray();
                string message = Encoding.UTF8.GetString(body);
                SendConnectionsBody sendConnectionsBody = _serializer.DeserializeSendConnectionRequestsBody(message);

                SendConnectionsCommand sendConnCommand = new SendConnectionsCommand(channel, eventArgs, sendConnectionsBody, sendConnectionsBody.StartOfWorkday, sendConnectionsBody.EndOfWorkday, sendConnectionsBody.TimeZoneId);
                await _sendConnectionsHandler.HandleAsync(sendConnCommand);
            }
            else
            {
                string networkTypeStr = networkType as string;
                _logger.LogError("Invalid network type specified {networkTypeStr}", networkTypeStr);
            }
        }

        #endregion

        #region FollowUpMessages

        public async Task OnFollowUpMessageEventReceivedAsync(object sender, BasicDeliverEventArgs eventArgs)
        {
            IModel channel = ((AsyncEventingBasicConsumer)sender).Model;

            byte[] body = eventArgs.Body.ToArray();
            string message = Encoding.UTF8.GetString(body);
            FollowUpMessageBody followUpMessages = _serializer.DeserializeFollowUpMessagesBody(message);

            FollowUpMessageCommand followUpMessageCommand = new FollowUpMessageCommand(channel, eventArgs, followUpMessages, followUpMessages.StartOfWorkday, followUpMessages.EndOfWorkday, followUpMessages.TimeZoneId);
            await _followUpHandler.HandleAsync(followUpMessageCommand);
        }

        #endregion

        #region ConnectionWithdraw

        public Task OnConnectionWithdrawEventReceivedAsync(object sender, BasicDeliverEventArgs eventArgs)
        {
            IModel channel = ((AsyncEventingBasicConsumer)sender).Model;
            string messageId = eventArgs.BasicProperties.MessageId;

            return Task.CompletedTask;

        }

        private void QueueConnectionWithdraw(string messageId, IModel channel, BasicDeliverEventArgs eventArgs)
        {
            //BackgroundJob.Enqueue<ICampaignManagerService>((x) => x.StartConnectionWithdraw(messageId));
        }

        private void StartConnectionWithdraw(string messageId)
        {

            //try and deserialize the response
            //IRabbitMQSerializer serializer = scope.ServiceProvider.GetRequiredService<IRabbitMQSerializer>();
            //byte[] body = eventArgs.Body.ToArray();
            //string message = Encoding.UTF8.GetString(body);
            //ConnectionWithdrawBody connectionWithdraw = serializer.DeserializeConnectionWithdrawBody(message);
            //Action ackOperation = default;
            //try
            //{
            //    ICampaignPhaseFacade campaignPhaseFacade = scope.ServiceProvider.GetRequiredService<ICampaignPhaseFacade>();
            //    HalOperationResult<IOperationResponse> operationResult = campaignPhaseFacade.ExecutePhase<IOperationResponse>(connectionWithdraw);

            //    if (operationResult.Succeeded == true)
            //    {
            //        _logger.LogInformation("ExecuteFollowUpMessagesPhase executed successfully. Acknowledging message");
            //        ackOperation = () => channel.BasicAck(eventArgs.DeliveryTag, false);
            //    }
            //    else
            //    {
            //        _logger.LogWarning("Executing Follow Up Messages Phase did not successfully succeeded. Negatively acknowledging the message and re-queuing it");
            //        ackOperation = () => channel.BasicNack(eventArgs.DeliveryTag, false, true);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    _logger.LogError(ex, "Exception occured while executing Follow Up Messages Phase. Negatively acknowledging the message and re-queuing it");
            //    ackOperation = () => channel.BasicNack(eventArgs.DeliveryTag, false, true);
            //}
            //finally
            //{
            //    ackOperation();
            //}
        }

        #endregion

        #region Networking

        public async Task OnNetworkingEventReceivedAsync(object sender, BasicDeliverEventArgs eventArgs)
        {
            IModel channel = ((AsyncEventingBasicConsumer)sender).Model;

            byte[] body = eventArgs.Body.ToArray();
            string message = Encoding.UTF8.GetString(body);
            NetworkingMessageBody messageBody = _serializer.DeserializeNetworkingMessageBody(message);

            // quorum queues are currently not supported in aws 
            //int deliveryCount = eventArgs.GetDeliveryCountHeaderValue();
            //_logger.LogInformation($"DeliveryCount value is {deliveryCount}");
            //messageBody.FailedDeliveryCount = deliveryCount;

            NetworkingCommand networkingCommand = new NetworkingCommand(channel, eventArgs, messageBody, messageBody.StartOfWorkday, messageBody.EndOfWorkday, messageBody.TimeZoneId);
            await _networkingHandler.HandleAsync(networkingCommand);
        }

        #endregion

        #region MonitorForNewAcceptedConnections

        public async Task OnMonitorForNewAcceptedConnectionsEventReceivedAsync(object sender, BasicDeliverEventArgs eventArgs)
        {
            IModel channel = ((AsyncEventingBasicConsumer)sender).Model;

            var headers = eventArgs.BasicProperties.Headers;
            headers.TryGetValue(RabbitMQConstants.MonitorNewAcceptedConnections.ExecuteType, out object executionTypeObj);

            byte[] networkTypeArr = executionTypeObj as byte[];

            string executionType = Encoding.UTF8.GetString(networkTypeArr);
            if (executionType == null)
            {
                throw new ArgumentException("A header value must be provided!");
            }

            byte[] body = eventArgs.Body.ToArray();
            string message = Encoding.UTF8.GetString(body);
            MonitorForNewAcceptedConnectionsBody messageBody = _serializer.DeserializeMonitorForNewAcceptedConnectionsBody(message);

            if (executionType == RabbitMQConstants.MonitorNewAcceptedConnections.ExecuteOffHoursScan)
            {
                _logger.LogInformation("[CheckOffHoursNewConnections] Executing CheckOffHoursNewConnections phase");
                CheckOffHoursNewConnectionsCommand offHoursCommand = new CheckOffHoursNewConnectionsCommand(channel, eventArgs, messageBody, messageBody.StartOfWorkday, messageBody.EndOfWorkday, messageBody.TimeZoneId);
                await _offHoursHandler.HandleAsync(offHoursCommand);
            }
            else if (executionType == RabbitMQConstants.MonitorNewAcceptedConnections.ExecutePhase)
            {
                _logger.LogInformation("[MonitorForNewConnections] Executing MonitorForNewConnections phase");
                MonitorForNewConnectionsCommand monitorCommand = new MonitorForNewConnectionsCommand(channel, eventArgs, messageBody, messageBody.StartOfWorkday, messageBody.EndOfWorkday, messageBody.TimeZoneId);
                await _monitorHandler.HandleAsync(monitorCommand);
            }
            else
            {
                _logger.LogInformation("Header called execution-type did not match any expected values. It's value was {executionType}", executionType);
            }
        }

        #endregion

        #region ScanProspectsForReplies
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
            string message = Encoding.UTF8.GetString(body);
            ScanProspectsForRepliesBody messageBody = _serializer.DeserializeScanProspectsForRepliesBody(message);

            if (networkType == RabbitMQConstants.ScanProspectsForReplies.ExecuteDeepScan)
            {
                DeepScanProspectsForRepliesCommand deepScanProspectsCommand = new DeepScanProspectsForRepliesCommand(channel, eventArgs, messageBody, messageBody.StartOfWorkday, messageBody.EndOfWorkday, messageBody.TimeZoneId);
                await _deepScanHandler.HandleAsync(deepScanProspectsCommand);
            }
            else if (networkType == RabbitMQConstants.ScanProspectsForReplies.ExecutePhase)
            {
                ScanProspectsForRepliesCommand scanProspectsCommand = new ScanProspectsForRepliesCommand(channel, eventArgs, messageBody, messageBody.StartOfWorkday, messageBody.EndOfWorkday, messageBody.TimeZoneId);
                await _scanHandler.HandleAsync(scanProspectsCommand);
            }
        }

        #endregion

        #region RestartApplication

        public async Task OnRestartApplicationEventReceivedAsync(object sender, BasicDeliverEventArgs eventArgs)
        {
            IModel channel = ((AsyncEventingBasicConsumer)sender).Model;
            RestartApplicationCommand restartCommand = new RestartApplicationCommand(channel, eventArgs);
            await _restartAppHandler.HandleAsync(restartCommand);
        }

        #endregion
    }
}
