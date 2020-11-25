using Microsoft.Extensions.Logging;
using System;
using Microsoft.Extensions.DependencyInjection;

namespace DataCore.DatabaseInitializer
{
    public class SeedData
    {
        public static void Populate(IServiceProvider serviceProvider, ILogger<DatabaseInitializer> logger)
        {
            DatabaseContext dbContext = serviceProvider.GetService<DatabaseContext>();

            Seed(dbContext, logger);
        }

        private static void Seed(DatabaseContext dbContext, ILogger<DatabaseInitializer> logger)
        {

        }
    }
}
