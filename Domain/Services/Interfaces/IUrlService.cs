using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Services.Interfaces
{
    public interface IUrlService
    {
        public string GetBaseServerUrl(string serviceDiscoveryName, string namespaceName);
    }
}
