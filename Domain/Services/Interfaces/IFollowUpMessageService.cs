﻿using Domain.Models.FollowUpMessage;
using Domain.Models.RabbitMQMessages;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Services.Interfaces
{
    public interface IFollowUpMessageService
    {
        Task ProcessSentFollowUpMessageAsync(SentFollowUpMessage item, FollowUpMessageBody message, CancellationToken ct = default);

    }
}
