using API.Configurations;
using API.Filters;
using API.Middlewares;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;            
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers()
                    .AddJsonOptionsConfiguration();                    

            services.AddConnectionProviders(Configuration)                    
                    .AddJsonWebTokenConfiguration(Configuration)
                    .AddAuthorizationConfiguration()
                    .AddCorsConfiguration(Configuration)
                    .AddApiBehaviorOptionsConfiguration()
                    .AddRepositoriesConfiguration()
                    .AddSupervisorConfiguration()
                    .AddIdentityConfiguration()                    
                    .AddRemoveNull204FormatterConfigration();

            // Configure application filters and conventions
            services.Configure<MvcOptions>(options =>
            {
                options.Filters.Clear();
                options.Filters.Add<InvalidModelStateFilter>(FilterOrders.RequestValidationFilter);

                AuthorizationPolicy defaultPolicy = new AuthorizationOptions().DefaultPolicy;
                options.Conventions.Add(new ControllerModelConvention(defaultPolicy));
            });            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseCors(APIConstants.Cors.AllowAll);                
            }
            else
            {
                app.UseCors(APIConstants.Cors.WithOrigins);
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();                
            }

            app.UsePathBase("/api");

            app.UseMiddleware<ErrorHandlingMiddleware>();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.SeedDatabase();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
