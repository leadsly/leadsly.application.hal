using Leadsly.Application.Model.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.LinkedInPages.SearchResultPage.Interfaces
{
    public interface IGetTotalNumberOfResults : IOperationResponse
    {
        public int NumberOfResults { get; set; }
    }
}
