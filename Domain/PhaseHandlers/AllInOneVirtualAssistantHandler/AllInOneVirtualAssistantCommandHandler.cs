using Domain.Executors;
using Domain.MQ.Messages;
using Domain.MQ.Services.Interfaces;
using Leadsly.Application.Model;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.PhaseHandlers.AllInOneVirtualAssistantHandler
{
    public class AllInOneVirtualAssistantCommandHandler : ICommandHandler<AllInOneVirtualAssistantCommand>
    {
        public AllInOneVirtualAssistantCommandHandler(
            ILogger<AllInOneVirtualAssistantCommandHandler> logger,
            IGetMQMessagesService service,
            IMessageExecutorHandler<AllInOneVirtualAssistantMessageBody> handler)
        {
            _logger = logger;
            _handler = handler;
            _service = service;
        }

        private readonly ILogger<AllInOneVirtualAssistantCommandHandler> _logger;
        private readonly IMessageExecutorHandler<AllInOneVirtualAssistantMessageBody> _handler;
        private readonly IGetMQMessagesService _service;

        public async Task HandleAsync(AllInOneVirtualAssistantCommand command)
        {
            IModel channel = command.Channel;
            BasicDeliverEventArgs eventArgs = command.EventArgs;
            channel.BasicAck(eventArgs.DeliveryTag, false);

            AllInOneVirtualAssistantMessageBody message = command.MessageBody as AllInOneVirtualAssistantMessageBody;

            GetNetworkingMessages(message);
            // this needs to be requested each time at the top of the hour
            // GetFollowUpMessages(message);

            bool succeeded = await _handler.ExecuteMessageAsync(message);

            if (succeeded == true)
            {
                _logger.LogDebug($"{nameof(AllInOneVirtualAssistantCommand)} phase finished executing successfully");
            }
            else
            {
                _logger.LogDebug($"{nameof(AllInOneVirtualAssistantCommand)} phase finished executing unsuccessfully");
            }
        }

        private void GetNetworkingMessages(AllInOneVirtualAssistantMessageBody message)
        {
            string queueNameIn = RabbitMQConstants.Networking.QueueName;
            Queue<NetworkingMessageBody> allMessages = _service.GetAllMessages<NetworkingMessageBody>(queueNameIn, message.HalId);
            if (allMessages == null)
            {
                _logger.LogDebug("There were no {0} messages found", nameof(NetworkingMessageBody));
                message.NetworkingMessages = new Queue<NetworkingMessageBody>();
            }
            else
            {
                message.NetworkingMessages = allMessages;
            }
        }

        //private void GetFollowUpMessages(AllInOneVirtualAssistantMessageBody message)
        //{
        //    string queueNameIn = RabbitMQConstants.FollowUpMessage.QueueName;
        //    Queue<FollowUpMessageBody> allMessages = _service.GetAllMessages<FollowUpMessageBody>(queueNameIn, message.HalId);
        //    if (allMessages == null)
        //    {
        //        _logger.LogDebug("There were no {0} messages found", nameof(FollowUpMessageBody));
        //        message.FollowUpMessages = new Queue<FollowUpMessageBody>();
        //    }
        //    else
        //    {
        //        message.FollowUpMessages = allMessages;
        //    }
        //}
    }
}
