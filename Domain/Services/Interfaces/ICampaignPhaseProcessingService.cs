using Leadsly.Application.Model;
using Leadsly.Application.Model.Requests.FromHal;
using Leadsly.Application.Model.Responses;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Services.Interfaces
{
    public interface ICampaignPhaseProcessingService
    {
        Task<HttpResponseMessage> ProcessNewConnectionsAsync(NewConnectionRequest request, CancellationToken ct = default);

        Task<HttpResponseMessage> ProcessProspectListAsync(ProspectListPhaseCompleteRequest request, CancellationToken ct = default);
    }
}
