using Domain.Providers.Interfaces;
using Leadsly.Application.Model.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Providers
{
    public class HalOperationConfigurationProvider : IHalOperationConfigurationProvider
    {
        public Task<HalOperationConfiguration> GetOperationConfigurationByIdAsync(string halId, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }
}
