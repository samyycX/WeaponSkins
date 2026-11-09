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
            Agents = source.Agents,
            MusicKits = source.MusicKits,
            Graffiti = source.Graffiti,
            Patches = source.Patches,
            Pins = source.Pins,
            Nametag = source.Nametag,
            Wear = source.Wear,
            Seed = source.Seed
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

        // A newly chosen skin's defaults.
        if (!CanUseNametag(data.SteamID)) result.Nametag = null;
        if (!CanUseWear(data.SteamID)) result.PaintkitWear = 0f;
        if (!CanUseSeed(data.SteamID)) result.PaintkitSeed = 0;

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
        if (!CanUseNametag(data.SteamID)) result.Nametag = null;
        if (!CanUseWear(data.SteamID)) result.PaintkitWear = 0f;
        if (!CanUseSeed(data.SteamID)) result.PaintkitSeed = 0;
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
        if (!CanUseWear(data.SteamID)) result.PaintkitWear = 0f;
        if (!CanUseSeed(data.SteamID)) result.PaintkitSeed = 0;
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

        if (!CanUseNametag(current.SteamID)) result.Nametag = current.Nametag;
        if (!CanUseWear(current.SteamID)) result.PaintkitWear = current.PaintkitWear;
        if (!CanUseSeed(current.SteamID)) result.PaintkitSeed = current.PaintkitSeed;

        return result;
    }

    public KnifeSkinData ApplyKnifeUpdateRules(KnifeSkinData current,
        KnifeSkinData updated)
    {
        var result = updated.DeepClone();
        if (!CanUseNametag(current.SteamID)) result.Nametag = current.Nametag;
        if (!CanUseWear(current.SteamID)) result.PaintkitWear = current.PaintkitWear;
        if (!CanUseSeed(current.SteamID)) result.PaintkitSeed = current.PaintkitSeed;
        return result;
    }

    public GloveData ApplyGloveUpdateRules(GloveData current,
        GloveData updated)
    {
        var result = updated.DeepClone();
        if (!CanUseWear(current.SteamID)) result.PaintkitWear = current.PaintkitWear;
        if (!CanUseSeed(current.SteamID)) result.PaintkitSeed = current.PaintkitSeed;
        return result;
    }

    public bool CanUseWeaponSkins(ulong steamId) => HasPermission(steamId, Config.WeaponSkins);

    public bool CanUseKnifeSkins(ulong steamId) => HasPermission(steamId, Config.KnifeSkins);

    public bool CanUseGloveSkins(ulong steamId) => HasPermission(steamId, Config.GloveSkins);

    public bool CanUseStickers(ulong steamId) => HasPermission(steamId, Config.Stickers);

    public bool CanUseKeychains(ulong steamId) => HasPermission(steamId, Config.Keychains);

    public bool CanUseAgents(ulong steamId) => HasPermission(steamId, Config.Agents);

    public bool CanUseMusicKits(ulong steamId) => HasPermission(steamId, Config.MusicKits);

    public bool CanUseGraffiti(ulong steamId) => HasPermission(steamId, Config.Graffiti);

    public bool CanUsePatches(ulong steamId) => HasPermission(steamId, Config.Patches);

    public bool CanUsePins(ulong steamId) => HasPermission(steamId, Config.Pins);

    public bool CanUseNametag(ulong steamId) => HasPermission(steamId, Config.Nametag);

    public bool CanUseWear(ulong steamId) => HasPermission(steamId, Config.Wear);

    public bool CanUseSeed(ulong steamId) => HasPermission(steamId, Config.Seed);

    private bool HasPermission(ulong steamId,
        string permission)
    {
        if (string.IsNullOrWhiteSpace(permission)) return true;
        return Core.Permission.PlayerHasPermission(steamId, permission);
    }
}
