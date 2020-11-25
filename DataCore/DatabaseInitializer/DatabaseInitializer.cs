using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataCore.DatabaseInitializer
{
    public class DatabaseInitializer
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            ILogger<DatabaseInitializer> logger = serviceProvider.GetRequiredService<ILogger<DatabaseInitializer>>();

            try
            {
                IdentitySeedData.Populate(serviceProvider, logger).Wait();
                logger.LogInformation("Identity seed data executed.");

                SeedData.Populate(serviceProvider, logger);
                logger.LogInformation("Seed data executed.");
            }
            catch(Exception ex)
            {
                logger.LogError(ex, "Error occured seeding database");
            }
        }
    }
}
