﻿using Domain.MQ.EventHandlers.Interfaces;
using Domain.MQ.Messages;
using Domain.PhaseHandlers.MonitorForNewConnectionsHandler;
using Leadsly.Application.Model;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Domain.MQ.EventHandlers
{
    public class MonitorForNewAcceptedConnectionsEventHandler : RabbitMQEventHandlerBase, IMonitorForNewAcceptedConnectionsEventHandler
    {
        public MonitorForNewAcceptedConnectionsEventHandler(
            ILogger<MonitorForNewAcceptedConnectionsEventHandler> logger,
            HalWorkCommandHandlerDecorator<CheckOffHoursNewConnectionsCommand> offHoursHandler,
            HalWorkCommandHandlerDecorator<MonitorForNewConnectionsCommand> monitorHandler
            ) : base(logger)
        {
            _logger = logger;
            _monitorHandler = monitorHandler;
            _offHoursHandler = offHoursHandler;
        }

        private readonly ILogger<MonitorForNewAcceptedConnectionsEventHandler> _logger;
        private readonly HalWorkCommandHandlerDecorator<CheckOffHoursNewConnectionsCommand> _offHoursHandler;
        private readonly HalWorkCommandHandlerDecorator<MonitorForNewConnectionsCommand> _monitorHandler;
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
            string rawMessage = Encoding.UTF8.GetString(body);

            if (executionType == RabbitMQConstants.MonitorNewAcceptedConnections.ExecuteOffHoursScan)
            {
                PublishMessageBody message = DeserializeMessage<CheckOffHoursNewConnectionsBody>(rawMessage);
                _logger.LogInformation("[CheckOffHoursNewConnections] Executing CheckOffHoursNewConnections phase");
                CheckOffHoursNewConnectionsCommand offHoursCommand = new CheckOffHoursNewConnectionsCommand(channel, eventArgs, message);
                await _offHoursHandler.HandleAsync(offHoursCommand);
            }
            else if (executionType == RabbitMQConstants.MonitorNewAcceptedConnections.ExecutePhase)
            {
                PublishMessageBody message = DeserializeMessage<MonitorForNewAcceptedConnectionsBody>(rawMessage);
                _logger.LogInformation("[MonitorForNewConnections] Executing MonitorForNewConnections phase");
                MonitorForNewConnectionsCommand monitorCommand = new MonitorForNewConnectionsCommand(channel, eventArgs, message, message.StartOfWorkday, message.EndOfWorkday, message.TimeZoneId);
                await _monitorHandler.HandleAsync(monitorCommand);
            }
            else
            {
                _logger.LogInformation("Header called execution-type did not match any expected values. It's value was {executionType}", executionType);
            }
        }
    }
}
