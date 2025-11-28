using Microsoft.Extensions.DependencyInjection;

using WeaponSkins.Services;

namespace WeaponSkins.Injections;

public static class MenuServiceInjection
{
    public static IServiceCollection AddMenuService(this IServiceCollection services)
    {
        services.AddSingleton<MenuService>();
        return services;
    }

    public static IServiceProvider UseMenuService(this IServiceProvider provider)
    {
        provider.GetRequiredService<MenuService>();
        return provider;
    }
}