using Leadsly.Application.Model;
using Leadsly.Application.Model.Requests.FromHal;
using Leadsly.Application.Model.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Services.Interfaces
{
    public interface ICampaignPhaseProcessingService
    {
        Task<HalOperationResult<T>> ProcessNewConnectionsAsync<T>(NewConnectionRequest request, CancellationToken ct = default) 
            where T : IOperationResponse;
    }
}
