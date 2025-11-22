using Microsoft.Extensions.DependencyInjection;

using WeaponSkins.Econ;

namespace WeaponSkins;

public static class EconServiceInjection
{
    public static IServiceCollection AddEconService(this IServiceCollection services)
    {
        return services.AddSingleton<EconService>();
    }

    public static IServiceProvider UseEconService(this IServiceProvider provider)
    {
        provider.GetRequiredService<EconService>();
        return provider;
    }
}