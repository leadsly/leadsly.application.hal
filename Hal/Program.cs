using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.IO;

namespace Hal
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //https://stackoverflow.com/questions/2031163/when-to-use-the-different-log-levels
            string env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(configuration).CreateLogger();

            try
            {
                Log.Information($"ASPNETCORE_ENVIORNMENT variable is: {env}.");

                CreateHostBuilder(args)
                    .Build()
                    .Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application start-up failed.");
                throw;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            try
            {
                return Host.CreateDefaultBuilder(args)
                           .UseSerilog()
                           .ConfigureWebHostDefaults(webBuilder =>
                           {
                               webBuilder.UseStartup<Startup>();
                           })
                           .ConfigureAppConfiguration((hostBuilderContext, builder) =>
                           {
                               IHostEnvironment env = hostBuilderContext.HostingEnvironment;
                               Log.Information($"Application is running in: {env.EnvironmentName} mode.");
                               if (env.IsProduction() || env.IsStaging())
                               {
                                   Log.Information("Using AWS Secrets Manager");
                                   builder.AddSecretsManager();
                               }
                           });
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host builder error.");
                throw;
            }
        }
    }
}
