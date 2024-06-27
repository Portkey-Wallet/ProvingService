using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProvingService.Application.Contracts;

namespace ProvingService.Domain.Sha256Hash;

public static class Extensions
{
    public static IServiceCollection AddSha256Hash(this IServiceCollection services)
    {
        services.AddSingleton<IIdentifierHashService, Sha256IdentifierHashService>();
        return services;
    }
}