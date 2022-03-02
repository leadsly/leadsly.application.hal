using Leadsly.Application.Model;
using Leadsly.Application.Model.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Pages
{
    public interface ILinkedInPage
    {
        public bool IsAuthenticationRequired { get; }        
        HalOperationResult<T> GoToPage<T>(string pageUrl)
            where T : IOperationResponse;
    }
}
