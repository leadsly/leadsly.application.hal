using Domain.Models.Requests;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Services.Interfaces
{
    public interface ILeadslyGridSidecartService
    {
        Task<HttpResponseMessage> CloneChromeProfileAsync(CloneChromeProfileRequest request, CancellationToken ct = default);
    }
}
