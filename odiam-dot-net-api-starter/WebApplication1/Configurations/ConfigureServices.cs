using DataCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Configurations
{
    public static class ConfigureConnections
    {
        public static IServiceProvider AddConnectionProviders(this IServiceCollection services, IConfiguration configuration)
        {
            string defaultConnection = configuration.GetConnectionString("DefaultConnection");

            services.AddDbContext<DatabaseContext>(options =>
            {
                options.UseSqlServer(connection);
            }, ServiceLifetime.Scoped);
        }
    }
}
