using Microsoft.Extensions.DependencyInjection;

using WeaponSkins.Services;

namespace WeaponSkins.Injections;

public static class AgentPatchServiceInjection
{
    public static IServiceCollection AddAgentPatchService(this IServiceCollection services)
    {
        return services.AddSingleton<AgentPatchService>();
    }

    public static IServiceProvider UseAgentPatchService(this IServiceProvider provider)
    {
        provider.GetRequiredService<AgentPatchService>();
        return provider;
    }
}
