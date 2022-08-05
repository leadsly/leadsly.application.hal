using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.Responses.Hal.Interfaces
{
    public interface INewInvitationsMyNetwork : IOperationResponse
    {
        public IReadOnlyCollection<IWebElement> NewConnections { get; }
    }
}
