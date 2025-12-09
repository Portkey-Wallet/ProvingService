using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProvingService.Application.Contracts;
using ProvingService.Domain.PoseidonHash;

namespace ProvingService.Domain.Sha256Hash;

public static class Extensions
{
    public static IServiceCollection AddPoseidonHash(this IServiceCollection services)
    {
        services.AddSingleton<IIdentifierHashService, PoseidonIdentifierHashService>();
        return services;
    }
}