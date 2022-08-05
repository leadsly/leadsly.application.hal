using Leadsly.Application.Model.Responses.Hal;

namespace Leadsly.Application.Model.Responses
{
    public interface IOperationResponse
    {
        public OperationInformation OperationInformation { get; set; }
    }
}
