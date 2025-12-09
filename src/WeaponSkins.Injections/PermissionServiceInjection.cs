using Microsoft.Extensions.DependencyInjection;

using WeaponSkins.Services;

namespace WeaponSkins.Injections;

public static class PermissionServiceInjection
{
    public static IServiceCollection AddItemPermissionService(this IServiceCollection services)
    {
        services.AddSingleton<ItemPermissionService>();
        return services;
    }

    public static IServiceProvider UseItemPermissionService(this IServiceProvider provider)
    {
        provider.GetRequiredService<ItemPermissionService>();
        return provider;
    }
}
