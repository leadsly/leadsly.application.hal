﻿using Domain.RabbitMQ.Interfaces;
using Domain.Services.Interfaces;
using Leadsly.Application.Model;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.PhaseConsumers.ScanProspectsForRepliesHandlers
{
    public class ScanProspectsForRepliesConsumerCommandHandler : IConsumeCommandHandler<ScanProspectsForRepliesConsumerCommand>
    {
        public ScanProspectsForRepliesConsumerCommandHandler(IRabbitMQManager rabbitMQManager, IPhaseEventHandlerService campaignManagerService)
        {
            _rabbitMQManager = rabbitMQManager;
            _campaignManagerService = campaignManagerService;
        }

        private readonly IPhaseEventHandlerService _campaignManagerService;
        private readonly IRabbitMQManager _rabbitMQManager;

        public Task ConsumeAsync(ScanProspectsForRepliesConsumerCommand command)
        {
            string queueNameIn = RabbitMQConstants.ScanProspectsForReplies.QueueName;
            string routingKeyIn = RabbitMQConstants.ScanProspectsForReplies.RoutingKey;
            string halId = command.HalId;

            AsyncEventHandler<BasicDeliverEventArgs> onEventFiredHandlerAsync = _campaignManagerService.OnScanProspectsForRepliesEventReceivedAsync;

            _rabbitMQManager.StartConsuming(queueNameIn, routingKeyIn, halId, onEventFiredHandlerAsync);

            return Task.CompletedTask;
        }
    }
}
