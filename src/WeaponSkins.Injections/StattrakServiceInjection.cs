using Microsoft.Extensions.DependencyInjection;

namespace WeaponSkins.Injections;

public static class StattrakServiceInjection
{
    public static IServiceCollection AddStattrakService(this IServiceCollection services)
    {
        return services.AddSingleton<StattrakService>();
    }

    public static IServiceProvider UseStattrakService(this IServiceProvider provider)
    {
        provider.GetRequiredService<StattrakService>();
        return provider;
    }
}