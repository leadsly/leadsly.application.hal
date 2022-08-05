using Leadsly.Application.Model.Responses.Hal.Interfaces;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leadsly.Application.Model.Responses.Hal.POMs
{
    public class NewInvitationsMyNetwork : INewInvitationsMyNetwork
    {
        public OperationInformation OperationInformation { get; set; }

        public IReadOnlyCollection<IWebElement> NewConnections { get; set; }
    }
}
