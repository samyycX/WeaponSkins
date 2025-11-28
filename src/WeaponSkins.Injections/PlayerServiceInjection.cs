using Microsoft.Extensions.DependencyInjection;
using WeaponSkins.Services;

namespace WeaponSkins.Injections;

public static class PlayerServiceInjection
{
  public static IServiceCollection AddPlayerService(this IServiceCollection services)
  {
    return services.AddSingleton<PlayerService>();
  }

  public static IServiceProvider UsePlayerService(this IServiceProvider provider)
  {
    provider.GetRequiredService<PlayerService>();
    return provider;
  }
} 