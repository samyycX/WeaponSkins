using Microsoft.Extensions.Logging;

using SwiftlyS2.Shared;
using SwiftlyS2.Shared.Players;

using WeaponSkins.Shared;

namespace WeaponSkins.Database;

public class EmptyStorageProvider : IStorageProvider
{
    public string Name => "Invalid";
    private ILogger<EmptyStorageProvider> Logger { get; init; }

    public EmptyStorageProvider(ILogger<EmptyStorageProvider> logger)
    {
        Logger = logger;
    }

    private void WarnEmpty()
    {
        Logger.LogWarning("Empty storage provider called. This is not supported.");
    }

    public Task StoreSkinsAsync(IEnumerable<WeaponSkinData> skins)
    {
        WarnEmpty();
        return Task.CompletedTask;
    }

    public Task<WeaponSkinData?> GetSkinAsync(ulong steamId,
        Team team,
        ushort definitionIndex)
    {
        WarnEmpty();
        return Task.FromResult<WeaponSkinData?>(null);
    }

    public Task<IEnumerable<WeaponSkinData>> GetSkinsAsync(ulong steamId)
    {
        WarnEmpty();
        return Task.FromResult<IEnumerable<WeaponSkinData>>(new List<WeaponSkinData>());
    }

    public Task<IEnumerable<WeaponSkinData>> GetAllSkinsAsync()
    {
        WarnEmpty();
        return Task.FromResult<IEnumerable<WeaponSkinData>>(new List<WeaponSkinData>());
    }

    public Task RemoveSkinAsync(ulong steamId,
        Team team,
        ushort definitionIndex)
    {
        WarnEmpty();
        return Task.CompletedTask;
    }

    public Task RemoveSkinsAsync(ulong steamId)
    {
        WarnEmpty();
        return Task.CompletedTask;
    }

    public Task StoreKnifesAsync(IEnumerable<KnifeSkinData> knives)
    {
        WarnEmpty();
        return Task.CompletedTask;
    }

    public Task<KnifeSkinData?> GetKnifeAsync(ulong steamId,
        Team team)
    {
        WarnEmpty();
        return Task.FromResult<KnifeSkinData?>(null);
    }

    public Task<IEnumerable<KnifeSkinData>> GetKnifesAsync(ulong steamId)
    {
        WarnEmpty();
        return Task.FromResult<IEnumerable<KnifeSkinData>>(new List<KnifeSkinData>());
    }

    public Task<IEnumerable<KnifeSkinData>> GetAllKnifesAsync()
    {
        WarnEmpty();
        return Task.FromResult<IEnumerable<KnifeSkinData>>(new List<KnifeSkinData>());
    }

    public Task RemoveKnifeAsync(ulong steamId,
        Team team)
    {
        WarnEmpty();
        return Task.CompletedTask;
    }

    public Task RemoveKnifesAsync(ulong steamId)
    {
        WarnEmpty();
        return Task.CompletedTask;
    }

    public Task StoreGlovesAsync(IEnumerable<GloveData> gloves)
    {
        WarnEmpty();
        return Task.CompletedTask;
    }

    public Task<GloveData?> GetGloveAsync(ulong steamId,
        Team team)
    {
        WarnEmpty();
        return Task.FromResult<GloveData?>(null);
    }

    public Task<IEnumerable<GloveData>> GetGlovesAsync(ulong steamId)
    {
        WarnEmpty();
        return Task.FromResult<IEnumerable<GloveData>>(new List<GloveData>());
    }

    public Task<IEnumerable<GloveData>> GetAllGlovesAsync()
    {
        WarnEmpty();
        return Task.FromResult<IEnumerable<GloveData>>(new List<GloveData>());
    }

    public Task RemoveGloveAsync(ulong steamId,
        Team team)
    {
        WarnEmpty();
        return Task.CompletedTask;
    }

    public Task RemoveGlovesAsync(ulong steamId)
    {
        WarnEmpty();
        return Task.CompletedTask;
    }
}