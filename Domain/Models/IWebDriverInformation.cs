using Leadsly.Application.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public interface IWebDriverInformation : IOperationResult
    {
        public string WebDriverId { get; set; }
    }
}
