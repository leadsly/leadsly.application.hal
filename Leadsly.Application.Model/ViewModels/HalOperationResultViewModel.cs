using Leadsly.Application.Model.ViewModels.Response;

namespace Leadsly.Application.Model.ViewModels
{
    public class HalOperationResultViewModel<T>
        where T : IOperationResponseViewModel
    {
        public ResultBaseViewModel OperationResults { get; set; } = new();

        /// <summary>
        /// Old way of passing data down. This became very cumbersome to constantly having to create interfaces that implemented T just to send some data back.
        /// </summary>
        public T Value{ get; set; }

        /// <summary>
        /// New way of passing data down. Very easy and quick
        /// </summary>
        public object Data { get; set; }
    }
}
