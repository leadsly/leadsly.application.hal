using Leadsly.Application.Model.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Domain.Providers.Interfaces
{
    public interface IHalOperationConfigurationProvider
    {
        public Task<HalOperationConfiguration> GetOperationConfigurationByIdAsync(string halId, CancellationToken ct = default);
    }
}
