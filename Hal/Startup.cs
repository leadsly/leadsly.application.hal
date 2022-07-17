using Domain;
using Domain.Services;
using Hal.Configurations;
using Hal.Middlewares;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Hal
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {

            Configuration = configuration;
            Environment = env;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers()
                    .AddJsonOptionsConfiguration();

            services.AddJsonWebTokenConfiguration(Configuration)
                    .AddAuthorizationConfiguration()
                    .AddCorsConfiguration(Configuration)
                    .AddApiBehaviorOptionsConfiguration()
                    .AddSupervisorConfiguration()
                    .AddSeleniumServicesConfiguration(Configuration)
                    .AddCommandHandlers()
                    .AddHttpContextAccessor()
                    .AddRepositoryConfiguration()
                    .AddRemoveNull204FormatterConfigration()
                    .AddRabbitMQConfiguration(Configuration)
                    .AddPageObjectModelsConfiguration()
                    .AddFacadesConfiguration()
                    .AddProvidersConfiguration()
                    .AddSerializersConfiguration()
                    .AddServicesConfiguration()
                    .AddHostedService<ConsumingHostedService>()
                    .AddMemoryCache();

            services.Configure<MvcOptions>(ApiDefaults.Configure);

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // if request does not contain api it will also work
            app.UseCors(ApiConstants.Cors.AllowAll);
            app.UsePathBase("/api");

            if (env.IsDevelopment())
            {
                app.UseCors(ApiConstants.Cors.AllowAll);
            }
            else
            {
                app.UseCors(ApiConstants.Cors.WithOrigins);
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseMiddleware<ErrorHandlingMiddleware>();

            app.UseHttpsRedirection();

            app.UseAuthentication();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseSerilogRequestLogging();

        }
    }
}
