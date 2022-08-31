using Leadsly.Application.Model.Requests;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Services.Interfaces.Api
{
    public interface IScanProspectsForRepliesServiceApi
    {
        Task<HttpResponseMessage> ProcessNewMessagesAsync(NewMessagesRequest request, CancellationToken ct = default);
    }
}
