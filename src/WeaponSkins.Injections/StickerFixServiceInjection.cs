using Microsoft.Extensions.DependencyInjection;

namespace WeaponSkins.Injections;

public static class StickerFixServiceInjection
{
    public static IServiceCollection AddStickerFixService(this IServiceCollection services)
    {
        return services.AddSingleton<StickerFixService>();
    }

    public static IServiceProvider UseStickerFixService(this IServiceProvider provider)
    {
        provider.GetRequiredService<StickerFixService>();
        return provider;
    }
}