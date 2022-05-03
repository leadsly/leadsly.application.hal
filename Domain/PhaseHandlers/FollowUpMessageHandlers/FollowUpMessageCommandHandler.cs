﻿using Domain.Facades.Interfaces;
using Domain.Serializers.Interfaces;
using Hangfire;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.Responses;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.PhaseHandlers.FollowUpMessageHandlers
{
    public class FollowUpMessageCommandHandler : ICommandHandler<FollowUpMessageCommand>
    {
        public FollowUpMessageCommandHandler(
            ICampaignPhaseFacade campaignPhaseFacade, 
            IRabbitMQSerializer serializer, 
            ILogger<FollowUpMessageCommandHandler> logger)
        {
            _campaignPhaseFacade = campaignPhaseFacade;
            _serializer = serializer;
            _logger = logger;
        }

        private readonly ILogger<FollowUpMessageCommandHandler> _logger;
        private readonly ICampaignPhaseFacade _campaignPhaseFacade;
        private readonly IRabbitMQSerializer _serializer;

        public async Task HandleAsync(FollowUpMessageCommand command)
        {
            IModel channel = command.Channel;
            BasicDeliverEventArgs args = command.EventArgs;

            byte[] body = command.EventArgs.Body.ToArray();
            string message = Encoding.UTF8.GetString(body);
            FollowUpMessageBody followUpMessages = _serializer.DeserializeFollowUpMessagesBody(message);
            try
            {
                await StartFollowUpMessagesAsync(followUpMessages);
                channel.BasicAck(args.DeliveryTag, false);
            }
            catch(Exception ex)
            {
                channel.BasicNack(args.DeliveryTag, false, true);
            }                        
        }

        private async Task StartFollowUpMessagesAsync(FollowUpMessageBody followUpMessages)
        {
            HalOperationResult<IOperationResponse> operationResult = await _campaignPhaseFacade.ExecutePhaseAsync<IOperationResponse>(followUpMessages);

            if (operationResult.Succeeded == true)
            {
                _logger.LogInformation("ExecuteFollowUpMessagesPhase executed successfully. Acknowledging message");                
            }
            else
            {
                _logger.LogWarning("Executing Follow Up Messages Phase did not successfully succeeded. Negatively acknowledging the message and re-queuing it");                
            }
        }

    }
}