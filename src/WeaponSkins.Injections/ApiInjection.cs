using Microsoft.Extensions.DependencyInjection;

namespace WeaponSkins.Injections;

public static class ApiInjection
{
    public static IServiceCollection AddApi(this IServiceCollection services)
    {
        return services.AddSingleton<WeaponSkinAPI>()
            .AddSingleton<WeaponSkinGetterAPI>();
    }

    public static IServiceProvider UseApi(this IServiceProvider provider)
    {
        provider.GetRequiredService<WeaponSkinAPI>();
        provider.GetRequiredService<WeaponSkinGetterAPI>();
        return provider;
    }
}