using Microsoft.Extensions.DependencyInjection;

using WeaponSkins.Database;

namespace WeaponSkins.Injections;

public static class DatabaseServiceInjection
{
    public static IServiceCollection AddDatabaseService(this IServiceCollection services)
    {
        return services
            .AddSingleton<DatabaseService>()
            .AddSingleton<DatabaseSynchronizeService>();
    }

    public static IServiceProvider UseDatabaseService(this IServiceProvider provider)
    {
        provider.GetRequiredService<DatabaseService>();
        provider.GetRequiredService<DatabaseSynchronizeService>();
        return provider;
    }
}