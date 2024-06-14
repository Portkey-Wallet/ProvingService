using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class Program
{
    static async Task Main(string[] args)
    {
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
            hostBuilder.ConfigureLogging(logging => logging.AddConsole(options =>
            {
                options.IncludeScopes = true;
                options.TimestampFormat = "yyyy-MM-dd hh:mm:ss";
            }));
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