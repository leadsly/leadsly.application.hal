using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                CreateHostBuilder(args).Build().Run();
            }
            catch(Exception ex)
            {
                Log.Fatal(ex, "Fatal error occured when invoking CreateWebHostBuilder");
                throw;
            }
            
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
               .WriteTo.Console()
               .WriteTo.File(Path.Combine(Directory.GetCurrentDirectory(), "api-logs.txt"))
               .MinimumLevel.Verbose()
               .CreateLogger();

            try
            {
                return Host.CreateDefaultBuilder(args)
                           .ConfigureWebHostDefaults(webBuilder =>
                           {
                                webBuilder.UseStartup<Startup>();
                           });
            }
            catch(Exception ex)
            {
                Log.Fatal(ex, "Host builder error.");
                throw;
            }
            finally
            {
                Log.Information("Finished starting up.");
            }
        }
            
    }
}
