using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;
using ProvingService.Application.Contracts;
using ProvingService.Application.PoseidonHash;
using ProvingService.Application.Sha256;

namespace ProvingService.Application;

public static class Extensions
{
    public static IServiceCollection AddGroth16Services(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ProverServerSettings>(configuration.GetSection("ProverServerSettings"));
        services.Configure<JwksSettings>(configuration.GetSection("JwksSettings"));
        services.Configure<CircuitSettings>(configuration.GetSection("CircuitSettings"));
        services.AddSingleton<IIdentifierHashService>(serviceProvider =>
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
        services.AddSingleton<IProvingService, ProvingService>();
        return services;
    }
}