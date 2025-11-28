using Microsoft.Extensions.DependencyInjection;

namespace WeaponSkins.Injections;

public static class NativeServiceInjection
{
    public static IServiceCollection AddNativeService(this IServiceCollection services)
    {
        return services.AddSingleton<NativeService>();
    }

    public static IServiceProvider UseNativeService(this IServiceProvider provider)
    {
        provider.GetRequiredService<NativeService>();
        return provider;
    }
}