using Microsoft.Extensions.DependencyInjection;

using WeaponSkins.Services;

namespace WeaponSkins.Injections;

public static class GraffitiServiceInjection
{
    public static IServiceCollection AddGraffitiService(this IServiceCollection services)
    {
        return services.AddSingleton<GraffitiService>();
    }

    public static IServiceProvider UseGraffitiService(this IServiceProvider provider)
    {
        provider.GetRequiredService<GraffitiService>();
        return provider;
    }
}
