using DataCore;
using DataCore.DatabaseInitializer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace API
{
    public static class ApplicationBuilderExtension
    {
        public static IApplicationBuilder SeedDatabase(this IApplicationBuilder app)
        {
            using (IServiceScope scope = app.ApplicationServices.CreateScope())
            {
                scope.ServiceProvider.GetService<DatabaseContext>().Database.Migrate();
                scope.ServiceProvider.GetService<DatabaseContext>().Database.EnsureCreated();

                DatabaseInitializer.Initialize(scope.ServiceProvider);
            }

            return app;
        }
    }
}
