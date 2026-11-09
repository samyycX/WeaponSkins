using Microsoft.Extensions.DependencyInjection;

using WeaponSkins.Services;

namespace WeaponSkins.Injections;

public static class PinServiceInjection
{
    public static IServiceCollection AddPinService(this IServiceCollection services)
    {
        return services.AddSingleton<PinService>();
    }

    public static IServiceProvider UsePinService(this IServiceProvider provider)
    {
        provider.GetRequiredService<PinService>();
        return provider;
    }
}
