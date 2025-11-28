using Microsoft.Extensions.DependencyInjection;

using WeaponSkins.Services;

namespace WeaponSkins.Injections;

public static class LocalizationServiceInjection
{
    public static IServiceCollection AddLocalizationService(this IServiceCollection services)
    {
        return services.AddSingleton<LocalizationService>();
    }

    public static IServiceProvider UseLocalizationService(this IServiceProvider provider)
    {
        provider.GetRequiredService<LocalizationService>();
        return provider;
    }
}   