using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.FeatureManagement;
using Microsoft.OpenApi.Models;
using ProvingService.Application;
using ProvingService.Application.Contracts;
using ProvingService.Application.PoseidonHash;
using ProvingService.Application.Sha256;

public class Startup
{
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddFeatureManagement();
        services.Configure<ProverServerSettings>(_configuration.GetSection("ProverServerSettings"));
        services.Configure<JwksSettings>(_configuration.GetSection("JwksSettings"));
        services.Configure<CircuitSettings>(_configuration.GetSection("CircuitSettings"));

        // TODO: Move these configurations into Application project
        services.AddTransient<IIdentifierHashService>(serviceProvider =>
        {
            var featureManager = serviceProvider.GetRequiredService<IFeatureManager>();
            if (featureManager.IsEnabledAsync("UsePoseidon").Result)
            {
                return new PoseidonIdentifierHashService();
            }

            return new Sha256IdentifierHashService();
        });

        services.AddSingleton<IJwksService, JwksService>();
        services.AddSingleton<IVerifyingService, VerifyingService>();
        services.AddSingleton<IProvingService, ProvingService.Application.ProvingService>();
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