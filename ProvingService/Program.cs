using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Formatting.Json;

public class Program
{
    static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console(new JsonFormatter())
            .WriteTo.File(new JsonFormatter(), "logs/proving-service-.log",
                rollingInterval: RollingInterval.Day, retainedFileCountLimit: 3,
                fileSizeLimitBytes: 2L * 1024 * 1024 * 1024)
            .CreateLogger();
        try
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile(
                    $"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json",
                    optional: true);
            var configuration = builder.Build();

            var hostBuilder = new HostBuilder();
            hostBuilder.ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
                webBuilder.UseConfiguration(configuration);
            });
            hostBuilder.ConfigureLogging(logging => logging.AddSerilog());
            var host = hostBuilder.Build();

            await host.StartAsync();

            await Task.Delay(-1);
        }
        catch (Exception ex)
        {
            Console.WriteLine("start fail, e: " + ex.Message);
        }
    }
}