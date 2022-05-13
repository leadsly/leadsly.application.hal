using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Extensions.AspNetCore.Configuration.Secrets;

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
            catch(Exception ex)
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

                               //if (env.IsProduction())
                               //{
                               //    IConfigurationRoot builtConfig = builder.Build();
                               //    SecretClient secretClient = new SecretClient(
                               //        new Uri(builtConfig["VaultUri"]),
                               //        new DefaultAzureCredential());
                               //    builder.AddAzureKeyVault(secretClient, new KeyVaultSecretManager());
                               //}
                           });
            }
            catch(Exception ex)
            {
                Log.Fatal(ex, "Host builder error.");
                throw;
            }
        }            
    }
}
