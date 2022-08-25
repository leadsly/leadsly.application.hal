using Domain.Models.Requests;
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
using UpdateSearchUrlProgressRequest = Leadsly.Application.Model.Requests.FromHal.UpdateSearchUrlProgressRequest;

namespace Domain.Services.Interfaces
{
    public interface ICampaignService
    {
        Task<HttpResponseMessage> GetLatestSentConnectionsUrlStatusesAsync(SearchUrlDetailsRequest request, CancellationToken ct = default);

        Task<HttpResponseMessage> GetSearchUrlProgressAsync(SearchUrlProgressRequest request, CancellationToken ct = default);

        Task<HttpResponseMessage> UpdateSendConnectionsUrlStatusesAsync(UpdateSearchUrlDetailsRequest request, CancellationToken ct = default);
        Task<HttpResponseMessage> UpdateSearchUrlProgressAsync(UpdateSearchUrlProgressRequest request, CancellationToken ct = default);

        Task<HttpResponseMessage> MarkCampaignExhausted(MarkCampaignExhaustedRequest request, CancellationToken ct = default);
    }
}
