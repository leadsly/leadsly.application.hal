using Leadsly.Application.Model;
using Leadsly.Application.Model.Campaigns;
using Leadsly.Application.Model.Requests.FromHal;
using Leadsly.Application.Model.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Services.Interfaces
{
    public interface ICampaignService
    {
        Task<HttpResponseMessage> GetLatestSentConnectionsUrlStatusesAsync(SentConnectionsUrlStatusRequest request, CancellationToken ct = default);

        Task<HttpResponseMessage> UpdateSendConnectionsUrlStatusesAsync(UpdateSentConnectionsUrlStatusRequest requestuest, CancellationToken ct = default);

        Task<HttpResponseMessage> MarkCampaignExhausted(MarkCampaignExhaustedRequest request, CancellationToken ct = default);
    }
}
