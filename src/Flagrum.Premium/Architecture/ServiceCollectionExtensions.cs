using Flagrum.Abstractions;
using Flagrum.Application.Features.Shared;
using Microsoft.Extensions.DependencyInjection;

namespace Flagrum.Premium.Architecture;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFlagrumPremium(this IServiceCollection services)
    {
        services.AddSingleton<IPremiumService, FreeService>();
        services.AddSingleton<IAuthenticationService, NoAuthenticationService>();

        return services;
    }
}