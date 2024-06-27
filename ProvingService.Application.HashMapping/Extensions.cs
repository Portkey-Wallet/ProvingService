using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProvingService.Application.Contracts.HashMapping;
using ProvingService.Domain.PoseidonHash;
using ProvingService.Domain.Sha256Hash;

namespace ProvingService.Application.HashMapping;

public static class Extensions
{
    public static IServiceCollection AddHashMapping(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<HashMappingSettings>(configuration.GetSection("HashMappingSettings"));
        services.Configure<CircuitSettingsForHashMapping>(
            configuration.GetSection("HashMappingSettings:CircuitSettings"));
        services.Configure<ProverServerSettingsForHashMapping>(
            configuration.GetSection("HashMappingSettings:ProverServerSettings"));
        services.AddSingleton<IProvingServiceForHashMapping, ProvingServiceForHashMapping>();
        services.AddSingleton<IVerifyingServiceForHashMapping, VerifyingServiceForHashMapping>();
        services.AddSingleton<PoseidonIdentifierHashService>();
        services.AddSingleton<Sha256IdentifierHashService>();
        return services;
    }
}