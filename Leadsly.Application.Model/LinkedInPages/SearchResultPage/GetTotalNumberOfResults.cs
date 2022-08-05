using Leadsly.Application.Model.LinkedInPages.SearchResultPage.Interfaces;
using Leadsly.Application.Model.Responses.Hal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.LinkedInPages.SearchResultPage
{
    public class GetTotalNumberOfResults : IGetTotalNumberOfResults
    {
        public OperationInformation OperationInformation { get; set; } = new();
        public int NumberOfResults { get; set; }
    }
}
