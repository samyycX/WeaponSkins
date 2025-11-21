using Microsoft.Extensions.DependencyInjection;

using WeaponSkins.Services;

namespace WeaponSkins;

public static class InventoryServiceInjection
{
    public static IServiceCollection AddInventoryService(this IServiceCollection services)
    {
        return services
        .AddSingleton<InventoryService>()
        .AddSingleton<InventoryUpdateService>();
    }

    public static IServiceProvider UseInventoryService(this IServiceProvider provider)
    {
        provider.GetRequiredService<InventoryService>();
        provider.GetRequiredService<InventoryUpdateService>();
        return provider;
    }
}