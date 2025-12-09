using System.Collections.Generic;
using System.Linq;

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
        options.OnChange(model => { Config = CloneConfig(model.ItemPermissions); });
    }

    private static ItemPermissionConfig CloneConfig(ItemPermissionConfig? source)
    {
        var config = new ItemPermissionConfig();
        if (source != null)
        {
            config.Stickers = source.Stickers?.ToDictionary(pair => pair.Key, pair => pair.Value) ?? new();
            config.Keychains = source.Keychains?.ToDictionary(pair => pair.Key, pair => pair.Value) ?? new();
        }

        return config;
    }

    public WeaponSkinData BuildRuntimeView(WeaponSkinData data)
    {
        var result = data.DeepClone();
        for (var slot = 0; slot < 6; slot++)
        {
            if (!CanUseSticker(result.SteamID, result.GetSticker(slot)?.Id ?? 0))
            {
                result.SetSticker(slot, null);
            }
        }

        if (!CanUseKeychain(result.SteamID, result.Keychain0?.Id ?? 0))
        {
            result.Keychain0 = null;
        }

        return result;
    }

    public WeaponSkinData ApplyUpdateRules(WeaponSkinData current,
        WeaponSkinData updated)
    {
        var result = updated.DeepClone();
        for (var slot = 0; slot < 6; slot++)
        {
            var previous = current.GetSticker(slot);
            var candidate = result.GetSticker(slot);
            if (!IsStickerChangeAllowed(current.SteamID, previous, candidate))
            {
                result.SetSticker(slot, previous?.DeepClone());
            }
        }

        if (!IsKeychainChangeAllowed(current.SteamID, current.Keychain0, result.Keychain0))
        {
            result.Keychain0 = current.Keychain0?.DeepClone();
        }

        return result;
    }

    public bool CanUseSticker(ulong steamId,
        int stickerId)
    {
        if (stickerId <= 0) return true;
        if (!TryGetStickerPermission(stickerId, out var permission)) return true;
        return HasPermission(steamId, permission);
    }

    public bool CanUseKeychain(ulong steamId,
        int keychainId)
    {
        if (keychainId <= 0) return true;
        if (!TryGetKeychainPermission(keychainId, out var permission)) return true;
        return HasPermission(steamId, permission);
    }

    private bool IsStickerChangeAllowed(ulong steamId,
        StickerData? previous,
        StickerData? candidate)
    {
        if (candidate == null) return true;
        if (previous != null && previous.Id == candidate.Id) return true;
        return CanUseSticker(steamId, candidate.Id);
    }

    private bool IsKeychainChangeAllowed(ulong steamId,
        KeychainData? previous,
        KeychainData? candidate)
    {
        if (candidate == null) return true;
        if (previous != null && previous.Id == candidate.Id) return true;
        return CanUseKeychain(steamId, candidate.Id);
    }

    private bool TryGetStickerPermission(int stickerId,
        out string permission)
    {
        permission = string.Empty;
        return Config.Stickers != null &&
               Config.Stickers.TryGetValue(stickerId, out permission) &&
               !string.IsNullOrWhiteSpace(permission);
    }

    private bool TryGetKeychainPermission(int keychainId,
        out string permission)
    {
        permission = string.Empty;
        return Config.Keychains != null &&
               Config.Keychains.TryGetValue(keychainId, out permission) &&
               !string.IsNullOrWhiteSpace(permission);
    }

    private bool HasPermission(ulong steamId,
        string permission)
    {
        if (string.IsNullOrWhiteSpace(permission)) return true;
        return Core.Permission != null && Core.Permission.HasPermission(steamId, permission);
    }
}
