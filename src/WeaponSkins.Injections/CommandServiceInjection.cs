using Microsoft.Extensions.DependencyInjection;

namespace WeaponSkins.Injections;

public static class CommandServiceInjection
{
    public static IServiceCollection AddCommandService(this IServiceCollection services)
    {
        return services.AddSingleton<CommandService>();
    }

    public static IServiceProvider UseCommandService(this IServiceProvider provider)
    {
        provider.GetRequiredService<CommandService>();
        return provider;
    }
}   