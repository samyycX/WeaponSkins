using Microsoft.Extensions.DependencyInjection;

namespace WeaponSkins.Injections;

public static class DataServiceInjection
{
    public static IServiceCollection AddDataService(this IServiceCollection services)
    {
        return services
            .AddSingleton<WeaponDataService>()
            .AddSingleton<KnifeDataService>()
            .AddSingleton<GloveDataService>()
            .AddSingleton<AgentDataService>()
            .AddSingleton<DataService>();
    }

    public static IServiceProvider UseDataService(this IServiceProvider provider)
    {
        provider.GetRequiredService<DataService>();
        return provider;
    }
}