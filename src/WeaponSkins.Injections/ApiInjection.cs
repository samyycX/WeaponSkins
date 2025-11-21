using Microsoft.Extensions.DependencyInjection;

namespace WeaponSkins;

public static class ApiInjection
{
    public static IServiceCollection AddApi(this IServiceCollection services)
    {
        return services.AddSingleton<WeaponSkinAPI>();
    }

    public static IServiceProvider UseApi(this IServiceProvider provider)
    {
        provider.GetRequiredService<WeaponSkinAPI>();
        return provider;
    }
}