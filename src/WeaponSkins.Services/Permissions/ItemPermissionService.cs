using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using SwiftlyS2.Shared;

using WeaponSkins.Configuration;
using WeaponSkins.Shared;

namespace WeaponSkins.Services;

public class ItemPermissionService
{
    private readonly ISwiftlyCore Core;
    private ItemPermissionConfig Config;

    public ItemPermissionService(ISwiftlyCore core,
        IOptionsMonitor<MainConfigModel> options)
    {
        Core = core;
        Config = CloneConfig(options.CurrentValue.ItemPermissions);
        options.OnChange(model =>
        {
            Config = CloneConfig(model.ItemPermissions);
        });
    }

    private static ItemPermissionConfig CloneConfig(ItemPermissionConfig? source)
    {
        if (source == null)
        {
            return new ItemPermissionConfig();
        }

        return new ItemPermissionConfig
        {
            WeaponSkins = source.WeaponSkins,
            KnifeSkins = source.KnifeSkins,
            GloveSkins = source.GloveSkins,
            Stickers = source.Stickers,
            Keychains = source.Keychains,
            Agents = source.Agents
        };
    }

    public bool TryBuildWeaponSkinView(WeaponSkinData data,
        [MaybeNullWhen(false)] out WeaponSkinData result)
    {
        result = null;
        if (!CanUseWeaponSkins(data.SteamID))
        {
            return false;
        }

        result = data.DeepClone();
        if (!CanUseStickers(data.SteamID))
        {
            for (var slot = 0; slot < 6; slot++)
            {
                result.SetSticker(slot, null);
            }
        }

        if (!CanUseKeychains(data.SteamID))
        {
            result.Keychain0 = null;
        }

        return true;
    }

    public bool TryBuildWeaponSkinsView(IEnumerable<WeaponSkinData> data,
        [MaybeNullWhen(false)] out IEnumerable<WeaponSkinData> result)
    {
        var results = new List<WeaponSkinData>();
        foreach (var skin in data)
        {
            if (TryBuildWeaponSkinView(skin, out var skin2))
            {
                results.Add(skin2);
            }
        }

        result = results;
        return true;
    }

    public bool TryBuildKnifeSkinView(KnifeSkinData data,
        [MaybeNullWhen(false)] out KnifeSkinData result)
    {
        if (!CanUseKnifeSkins(data.SteamID))
        {
            result = null;
            return false;
        }

        result = data.DeepClone();
        return true;
    }

    public bool TryBuildKnifeSkinsView(IEnumerable<KnifeSkinData> data,
        [MaybeNullWhen(false)] out IEnumerable<KnifeSkinData> result)
    {
        var results = new List<KnifeSkinData>();
        foreach (var knife in data)
        {
            if (TryBuildKnifeSkinView(knife, out var knife2))
            {
                results.Add(knife2);
            }
        }

        result = results;
        return true;
    }

    public bool TryBuildGloveView(GloveData data,
        [MaybeNullWhen(false)] out GloveData result)
    {
        if (!CanUseGloveSkins(data.SteamID))
        {
            result = null;
            return false;
        }

        result = data.DeepClone();
        return true;
    }

    public bool TryBuildGlovesView(IEnumerable<GloveData> data,
        [MaybeNullWhen(false)] out IEnumerable<GloveData> result)
    {
        var results = new List<GloveData>();
        foreach (var glove in data)
        {
            if (TryBuildGloveView(glove, out var glove2))
            {
                results.Add(glove2);
            }
        }

        result = results;
        return true;
    }

    public WeaponSkinData ApplyWeaponUpdateRules(WeaponSkinData current,
        WeaponSkinData updated)
    {
        if (!CanUseWeaponSkins(current.SteamID))
        {
            return current.DeepClone();
        }

        var result = updated.DeepClone();
        if (!CanUseStickers(current.SteamID))
        {
            for (var slot = 0; slot < 6; slot++)
            {
                result.SetSticker(slot, current.GetSticker(slot)?.DeepClone());
            }
        }

        if (!CanUseKeychains(current.SteamID))
        {
            result.Keychain0 = current.Keychain0?.DeepClone();
        }

        return result;
    }

    public bool CanUseWeaponSkins(ulong steamId) => HasPermission(steamId, Config.WeaponSkins);

    public bool CanUseKnifeSkins(ulong steamId) => HasPermission(steamId, Config.KnifeSkins);

    public bool CanUseGloveSkins(ulong steamId) => HasPermission(steamId, Config.GloveSkins);

    public bool CanUseStickers(ulong steamId) => HasPermission(steamId, Config.Stickers);

    public bool CanUseKeychains(ulong steamId) => HasPermission(steamId, Config.Keychains);

    public bool CanUseAgents(ulong steamId) => HasPermission(steamId, Config.Agents);

    private bool HasPermission(ulong steamId,
        string permission)
    {
        if (string.IsNullOrWhiteSpace(permission)) return true;
        return Core.Permission.PlayerHasPermission(steamId, permission);
    }
}
