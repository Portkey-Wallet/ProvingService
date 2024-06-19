using System;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using ProvingService.Controllers;
using ProvingService.Services;

public class Startup
{
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.Configure<ProverServerSettings>(_configuration.GetSection("ProverServerSettings"));
        services.Configure<JwksSettings>(_configuration.GetSection("JwksSettings"));
        services.Configure<CircuitSettings>(_configuration.GetSection("CircuitSettings"));

        services.AddSingleton<IJwksService, JwksService>();
        services.AddSingleton<IVerifyingService, VerifyingService>();
        services.AddSingleton<IProvingService, ProvingService.Services.ProvingService>();
        services.AddHttpClient();

        services.AddControllers();

        // Register the Swagger generator
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "ProvingService API", Version = "v1" });
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        // Enable middleware to serve generated Swagger as Helper JSON endpoint
        app.UseSwagger();

        // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
        // specifying the Swagger JSON endpoint
        app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "ProvingService API V1"); });

        app.UseRouting();

        app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
    }
}