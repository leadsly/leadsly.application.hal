﻿using Domain.Facades.Interfaces;
using Domain.PhaseConsumers;
using Domain.PhaseHandlers.FollowUpMessageHandlers;
using Domain.PhaseHandlers.MonitorForNewConnectionsHandler;
using Domain.PhaseHandlers.NetworkingConnectionsHandler;
using Domain.PhaseHandlers.ScanProspectsForRepliesHandler;
using Domain.PhaseHandlers.SendConnectionsHandler;
using Domain.Providers.Interfaces;
using Domain.Serializers.Interfaces;
using Domain.Services.Interfaces;
using Hangfire;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.RabbitMQ;
using Leadsly.Application.Model.Responses;
using Leadsly.Application.Model.WebDriver;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Services
{
    public class PhaseEventHandlerService : IPhaseEventHandlerService
    {
        public PhaseEventHandlerService(
            ILogger<PhaseEventHandlerService> logger,             
            HalWorkCommandHandlerDecorator<FollowUpMessageCommand> followUpHandler,
            HalWorkCommandHandlerDecorator<SendConnectionsCommand> sendConnectionsHandler,
            HalWorkCommandHandlerDecorator<ProspectListCommand> prospectListHandler,
            HalWorkCommandHandlerDecorator<MonitorForNewConnectionsCommand> monitorHandler,
            HalWorkCommandHandlerDecorator<ScanProspectsForRepliesCommand> scanHandler,
            HalWorkCommandHandlerDecorator<CheckOffHoursNewConnectionsCommand> offHoursHandler,
            HalWorkCommandHandlerDecorator<DeepScanProspectsForRepliesCommand> deepScanHandler,
            IWebDriverProvider webDriverProvider
            )
        {
            _logger = logger;
            _webDriverProvider = webDriverProvider;
            _followUpHandler = followUpHandler;
            _sendConnectionsHandler = sendConnectionsHandler;
            _monitorHandler = monitorHandler;
            _prospectListHandler = prospectListHandler;
            _scanHandler = scanHandler;
            _offHoursHandler = offHoursHandler;
            _deepScanHandler = deepScanHandler;
        }

        private readonly HalWorkCommandHandlerDecorator<CheckOffHoursNewConnectionsCommand> _offHoursHandler;
        private readonly HalWorkCommandHandlerDecorator<DeepScanProspectsForRepliesCommand> _deepScanHandler;
        private readonly HalWorkCommandHandlerDecorator<ScanProspectsForRepliesCommand> _scanHandler;
        private readonly HalWorkCommandHandlerDecorator<MonitorForNewConnectionsCommand> _monitorHandler;
        private readonly HalWorkCommandHandlerDecorator<FollowUpMessageCommand> _followUpHandler;
        private readonly HalWorkCommandHandlerDecorator<SendConnectionsCommand> _sendConnectionsHandler;
        private readonly HalWorkCommandHandlerDecorator<ProspectListCommand> _prospectListHandler;
        private readonly ILogger<PhaseEventHandlerService> _logger;        
        private readonly IWebDriverProvider _webDriverProvider;

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
                ProspectListCommand prospectListCommand = new ProspectListCommand(channel, eventArgs);
                await _prospectListHandler.HandleAsync(prospectListCommand);
            }
            else if((networkType as string) == RabbitMQConstants.NetworkingConnections.SendConnectionRequests)
            {
                SendConnectionsCommand sendConnCommand = new SendConnectionsCommand(channel, eventArgs);
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
            FollowUpMessageCommand followUpMessageCommand = new FollowUpMessageCommand(channel, eventArgs);
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

            if(executionType == RabbitMQConstants.MonitorNewAcceptedConnections.ExecuteOffHoursScan)
            {
                CheckOffHoursNewConnectionsCommand offHoursCommand = new CheckOffHoursNewConnectionsCommand(channel, eventArgs);
                await _offHoursHandler.HandleAsync(offHoursCommand);
            }
            else if(executionType == RabbitMQConstants.MonitorNewAcceptedConnections.ExecutePhase)
            {
                if (_webDriverProvider.WebDriverExists(BrowserPurpose.MonitorForNewAcceptedConnections) == false)
                {
                    MonitorForNewConnectionsCommand monitorCommand = new MonitorForNewConnectionsCommand(channel, eventArgs);
                    await _monitorHandler.HandleAsync(monitorCommand);
                }
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

            if (networkType == RabbitMQConstants.ScanProspectsForReplies.ExecuteDeepScan)
            {
                DeepScanProspectsForRepliesCommand deepScanProspectsCommand = new DeepScanProspectsForRepliesCommand(channel, eventArgs);
                await _deepScanHandler.HandleAsync(deepScanProspectsCommand);
            }
            else if (networkType == RabbitMQConstants.ScanProspectsForReplies.ExecutePhase)
            {
                if (_webDriverProvider.WebDriverExists(BrowserPurpose.ScanForReplies) == false)
                {
                    ScanProspectsForRepliesCommand scanProspectsCommand = new ScanProspectsForRepliesCommand(channel, eventArgs);
                    await _scanHandler.HandleAsync(scanProspectsCommand);
                }
                
            }
        }

        #endregion
    }
}
