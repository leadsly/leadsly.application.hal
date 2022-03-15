﻿using Domain.Facades.Interfaces;
using Domain.Providers.Campaigns.Interfaces;
using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.Responses;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Facades
{
    public class CampaignPhaseFacade : ICampaignPhaseFacade
    {
        public CampaignPhaseFacade(IFollowUpMessagesProvider followUpMessagesProvider, ILogger<CampaignPhaseFacade> logger)
        {
            _followUpMessagesProvider = followUpMessagesProvider;
            _logger = logger;
        }

        private readonly IFollowUpMessagesProvider _followUpMessagesProvider;
        private readonly ILogger<CampaignPhaseFacade> _logger;

        public HalOperationResult<T> ExecuteFollowUpMessagesPhase<T>(FollowUpMessagesBody message)
            where T : IOperationResponse
        {
            return _followUpMessagesProvider.ExecutePhase<T>(message);
        }
    }
}
