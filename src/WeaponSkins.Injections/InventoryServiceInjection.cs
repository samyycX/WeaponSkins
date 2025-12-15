using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using WeaponSkins.Configuration;
using WeaponSkins.Services;

namespace WeaponSkins.Injections;

public static class InventoryServiceInjection
{
    public static IServiceCollection AddInventoryService(this IServiceCollection services)
    {
        return services
            .AddSingleton<InventoryService>()
            .AddSingleton<InventoryUpdateService>()
            .AddSingleton<HookInventoryUpdateService>()
            .AddSingleton<IInventoryUpdateService>(provider =>
            {
                var backend = provider.GetRequiredService<IOptionsMonitor<MainConfigModel>>().CurrentValue.InventoryUpdateBackend;
                return backend switch
                {
                    "hook" => provider.GetRequiredService<HookInventoryUpdateService>(),
                    "inventory" => provider.GetRequiredService<InventoryUpdateService>(),
                    _ => throw new InvalidOperationException($"Invalid inventory update backend: {backend}")
                };
            });
    }

    public static IServiceProvider UseInventoryService(this IServiceProvider provider)
    {
        provider.GetRequiredService<InventoryService>();
        provider.GetRequiredService<IInventoryUpdateService>();
        return provider;
    }
}